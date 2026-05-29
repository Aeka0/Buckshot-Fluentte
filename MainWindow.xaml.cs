using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using ControlAppearance = Wpf.Ui.Controls.ControlAppearance;

namespace BuckshotFluentte;

// ==================== Enums ====================

public enum GamePhase
{
    Idle,
    ShowingLoaded,
    PlayerTurnIdle,
    PlayerTurnArmed,
    PlayerUsingItem,
    DealerTurn,
    GameOver
}

public enum ItemType
{
    None,
    Handsaw,
    Medicine,
    Phone,
    Beer,
    Cigarette,
    Magnifier,
    Inverter,
    Adrenaline,
    Handcuffs
}

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    // ==================== Item Emoji Lookup ====================

    private static readonly Dictionary<ItemType, string> ItemEmoji = new()
    {
        [ItemType.Handsaw]    = "\U0001F52A",
        [ItemType.Medicine]   = "\U0001F48A",
        [ItemType.Phone]      = "\U0001F4F1",
        [ItemType.Beer]       = "\U0001F37A",
        [ItemType.Cigarette]  = "\U0001F6AC",
        [ItemType.Magnifier]  = "\U0001F50D",
        [ItemType.Inverter]   = "\U0001F39B\uFE0F",
        [ItemType.Adrenaline] = "\U0001F489",
        [ItemType.Handcuffs]  = "\u26D3\uFE0F",
    };

    private static readonly Dictionary<ItemType, string> ItemSoundFile = new()
    {
        [ItemType.Handsaw]    = "item_handsaw.ogg",
        [ItemType.Medicine]   = "item_medicine.ogg",
        [ItemType.Phone]      = "item_phone.ogg",
        [ItemType.Beer]       = "item_beer.ogg",
        [ItemType.Cigarette]  = "item_smoke.ogg",
        [ItemType.Magnifier]  = "item_magnifier.ogg",
        [ItemType.Inverter]   = "item_inverter.ogg",
        [ItemType.Adrenaline] = "item_adrenaline.ogg",
        [ItemType.Handcuffs]  = "item_handcuffs.ogg",
    };

    private static readonly ItemType[] AllItemTypes =
    {
        ItemType.Handsaw, ItemType.Medicine, ItemType.Phone, ItemType.Beer,
        ItemType.Cigarette, ItemType.Magnifier, ItemType.Inverter,
        ItemType.Adrenaline, ItemType.Handcuffs
    };

    // ==================== Game State ====================

    private GamePhase _phase;
    private int _gameCount;
    private int _roundCount;
    private int _dealerCharges;
    private int _playerCharges;
    private int _maxCharges;
    private List<bool> _loaded = new();
    private int _money;
    private int _bonus;
    private readonly Random _rng = new();
    private DispatcherTimer? _bonusTimer;

    // ==================== Item State ====================

    private ItemType[] _dealerItems = new ItemType[8];
    private ItemType[] _playerItems = new ItemType[8];
    private bool _handsawActive;
    private bool _handsawUsedThisCycle;
    private bool _handcuffsActive;
    private bool _handcuffsUsedThisCycle;
    private bool _adrenalineMode;
    private bool? _dealerKnowsCurrent;

    // ==================== Stats ====================

    private int _statGameActions;
    private int _statGameLiveHits;
    private int _statGameShellsEjected;
    private int _statGameCigarettesPlayer;
    private int _statGameBeersPlayer;

    private int _statTotalActions;
    private int _statTotalLiveHits;
    private int _statTotalShellsEjected;
    private int _statTotalCigarettesPlayer;
    private int _statTotalBeersPlayer;

    // ==================== Async Cancellation ====================

    private CancellationTokenSource _cts = new();
    private string _themeMode = "system";
    private bool _isWatchingSystemTheme;

    // ==================== UI Arrays ====================

    private Button[] _dealerSlotButtons = null!;
    private Button[] _playerSlotButtons = null!;

    // ==================== Constructor ====================

    public MainWindow()
    {
        InitializeComponent();

        _dealerSlotButtons = new[] { BtnSlotA1, BtnSlotA2, BtnSlotA3, BtnSlotA4, BtnSlotA5, BtnSlotA6, BtnSlotA7, BtnSlotA8 };
        _playerSlotButtons = new[] { BtnSlotB1, BtnSlotB2, BtnSlotB3, BtnSlotB4, BtnSlotB5, BtnSlotB6, BtnSlotB7, BtnSlotB8 };

        InitializeLanguage();

        _bonusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _bonusTimer.Tick += BonusTimer_Tick;

        // Initialize emoji content for main buttons
        SetEmoji(BtnTopAction, "\U0001F608"); // 😈 Dealer
        SetEmoji(BtnBottomAction, "\U0001F636"); // 😶 Player

        // Apply theme (default: follow system)
        ApplyTheme();
        Loaded += MainWindow_Loaded;

        ResetGame();
        UpdateUILanguage();
    }

    // ==================== i18n ====================

    private void InitializeLanguage()
    {
        Loc.Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals(
            "zh",
            StringComparison.OrdinalIgnoreCase) ? "zh" : "en";
        UpdateLanguageMenuSelection();
    }

    private void UpdateLanguageMenuSelection()
    {
        MenuLangEn.IsChecked = Loc.Language == "en";
        MenuLangZh.IsChecked = Loc.Language == "zh";
    }

    private void UpdateUILanguage()
    {
        MenuGameBar.Header = Loc.MenuGame;
        MenuNewGame.Header = Loc.MenuNewGame;
        MenuClose.Header = Loc.MenuClose;
        MenuSettingsBar.Header = Loc.MenuSettings;
        MenuLanguage.Header = Loc.MenuLanguage;
        MenuAbout.Header = Loc.MenuAbout;
        MenuTheme.Header = Loc.MenuTheme;
        MenuThemeSystem.Header = Loc.ThemeSystem;
        MenuThemeDark.Header = Loc.ThemeDark;
        MenuThemeLight.Header = Loc.ThemeLight;

        LblGame.Text = Loc.LabelGame;
        LblRound.Text = Loc.LabelRound;
        LblMoney.Text = Loc.LabelMoney;
        LblCharges.Text = Loc.LabelCharges;
        LblDealer.Text = Loc.LabelDealer;
        LblPlayer.Text = Loc.LabelPlayer;
        LblLoaded.Text = (_phase == GamePhase.Idle || _phase == GamePhase.ShowingLoaded) ? Loc.LabelLoaded : Loc.LabelLastShell;
        LblActionLog.Text = Loc.LabelActionLog;

        TxtMoneyNum.Text = $"{_money}$";

        TxtTurnInfo.Text = _phase switch
        {
            GamePhase.Idle => Loc.TurnAwaiting,
            GamePhase.ShowingLoaded => Loc.TurnLoading,
            GamePhase.PlayerTurnIdle or GamePhase.PlayerTurnArmed => Loc.TurnYour,
            GamePhase.DealerTurn => Loc.TurnDealer,
            GamePhase.GameOver => Loc.TurnGameOver,
            _ => TxtTurnInfo.Text
        };

        TxtHint.Text = "";
    }

    // ==================== Button Style Helpers ====================

    private void SetTargetButtonsAccent(bool accent)
    {
        var appearance = accent ? ControlAppearance.Primary : ControlAppearance.Transparent;
        BtnTopAction.Appearance = appearance;
        BtnBottomAction.Appearance = appearance;
    }

    // ==================== Game Lifecycle ====================

    private void ResetGame()
    {
        // Cancel any in-flight async operations (loading, distribution, etc.)
        _cts.Cancel();
        _cts = new CancellationTokenSource();

        _phase = GamePhase.Idle;
        _gameCount = 0;
        _roundCount = 0;
        _dealerCharges = 0;
        _playerCharges = 0;
        _maxCharges = 0;
        _loaded.Clear();
        _money = 0;
        _bonus = 0;
        _handsawActive = false;
        _handsawUsedThisCycle = false;
        _handcuffsActive = false;
        _handcuffsUsedThisCycle = false;
        _adrenalineMode = false;
        _dealerKnowsCurrent = null;

        ResetAllStats();

        _bonusTimer?.Stop();
        ClearAllItems();

        TxtGameNum.Text = "0";
        TxtRoundNum.Text = "0";
        TxtMoneyNum.Text = "0$";
        TxtChargeDealer.Text = "";
        TxtChargePlayer.Text = "";
        LblLoaded.Text = Loc.LabelLoaded;
        TxtLoaded.Text = "";
        TxtTurnInfo.Text = Loc.TurnAwaiting;
        TxtHint.Text = "";

        SetEmoji(BtnCenter, "\U0001F4C4");
        BtnCenter.Appearance = ControlAppearance.Primary; // Accent for waiver
        SetTargetButtonsAccent(false);
        TxtLog.Text = Loc.LogSignWaiver;
    }

    private void ResetPerGameStats()
    {
        _statGameActions = 0;
        _statGameLiveHits = 0;
        _statGameShellsEjected = 0;
        _statGameCigarettesPlayer = 0;
        _statGameBeersPlayer = 0;
    }

    private void ResetAllStats()
    {
        ResetPerGameStats();
        _statTotalActions = 0;
        _statTotalLiveHits = 0;
        _statTotalShellsEjected = 0;
        _statTotalCigarettesPlayer = 0;
        _statTotalBeersPlayer = 0;
    }

    private void StartGame()
    {
        _gameCount = 1;
        _roundCount = 1;
        _money = 0;
        ResetPerGameStats();
        UpdateCounters();

        SetEmoji(BtnCenter, "\U0001F52B");
        BtnCenter.Appearance = ControlAppearance.Secondary; // normal during transition
        AppendLog(Loc.LogGameStart);
        TxtHint.Text = "";

        _bonus = 70000;

        StartRound();
    }

    private void StartRound()
    {
        int charges = _rng.Next(2, 7);
        _dealerCharges = charges;
        _playerCharges = charges;
        _maxCharges = charges;
        _handsawActive = false;
        _handsawUsedThisCycle = false;
        _handcuffsActive = false;
        _handcuffsUsedThisCycle = false;
        _dealerKnowsCurrent = null;

        UpdateChargesUI();
        AppendLog(Loc.LogRoundBegins(_roundCount, charges));

        _ = GenerateAndShowLoadedAsync();
    }

    private async Task GenerateAndShowLoadedAsync()
    {
        _phase = GamePhase.ShowingLoaded;
        BtnCenter.Appearance = ControlAppearance.Secondary;
        SetTargetButtonsAccent(false);

        var token = _cts.Token;

        int count = _rng.Next(2, 9);
        var shells = new List<bool>();
        for (int i = 0; i < count; i++)
            shells.Add(_rng.Next(2) == 1);

        int liveCount = shells.Count(s => s);
        int blankCount = shells.Count(s => !s);
        if (liveCount == 0) shells[_rng.Next(count)] = true;
        else if (blankCount == 0) shells[_rng.Next(count)] = false;

        liveCount = shells.Count(s => s);
        blankCount = shells.Count(s => !s);

        LblLoaded.Text = Loc.LabelLoaded;
        TxtLoaded.Text = ShellsToString(shells);
        TxtTurnInfo.Text = Loc.TurnLoading;
        Sfx.Play("ammo_refresh.ogg");

        AppendLog(Loc.LogLoaded(count, liveCount, blankCount));

        await Task.Delay(5000);
        if (token.IsCancellationRequested) return;

        for (int i = shells.Count; i > 0; i--)
        {
            Sfx.Play(shells[i - 1] ? "ammo_loadlive.ogg" : "ammo_loadblank.ogg");
            TxtLoaded.Text = ShellsToString(shells.Take(i - 1).ToList());
            await Task.Delay(200);
            if (token.IsCancellationRequested) return;
        }

        LblLoaded.Text = Loc.LabelLastShell;
        TxtLoaded.Text = Loc.LastShellAwaiting;
        _ = Sfx.PlayDelayedAsync("pump.ogg", 250);

        Shuffle(shells);
        _loaded = shells;
        _dealerKnowsCurrent = null;

        await DistributeItemsAsync();

        if (token.IsCancellationRequested) return;

        SetPlayerTurn();
    }

    // ==================== Item Distribution ====================

    private async Task DistributeItemsAsync()
    {
        await DistributeItemsForSide(_dealerItems, _dealerSlotButtons);
        await DistributeItemsForSide(_playerItems, _playerSlotButtons);
    }

    private async Task DistributeItemsForSide(ItemType[] items, Button[] buttons)
    {
        var token = _cts.Token;
        int emptySlots = items.Count(i => i == ItemType.None);
        int toAdd = Math.Min(4, emptySlots);

        for (int added = 0; added < toAdd; added++)
        {
            var empties = new List<int>();
            for (int i = 0; i < 8; i++)
                if (items[i] == ItemType.None) empties.Add(i);
            if (empties.Count == 0) break;

            int slot = empties[_rng.Next(empties.Count)];
            ItemType item = AllItemTypes[_rng.Next(AllItemTypes.Length)];
            items[slot] = item;
            UpdateSlotUI(buttons, items, slot);

            await Task.Delay(150);
            if (token.IsCancellationRequested) return;
        }
    }

    // ==================== Turn Management ====================

    private void SetPlayerTurn()
    {
        _phase = GamePhase.PlayerTurnIdle;
        _handsawActive = false;
        _handsawUsedThisCycle = false;
        _handcuffsUsedThisCycle = false;
        _adrenalineMode = false;
        TxtTurnInfo.Text = Loc.TurnYour;
        AppendLog(Loc.LogYourTurn);

        BtnCenter.Appearance = ControlAppearance.Primary;
        SetTargetButtonsAccent(false);

        _bonusTimer?.Start();
    }

    private void SetDealerTurn()
    {
        _phase = GamePhase.DealerTurn;
        _handsawActive = false;
        _handsawUsedThisCycle = false;
        _adrenalineMode = false;
        TxtTurnInfo.Text = Loc.TurnDealer;
        AppendLog(Loc.LogDealerTurn);

        BtnCenter.Appearance = ControlAppearance.Secondary;
        SetTargetButtonsAccent(false);

        _bonusTimer?.Stop();

        _ = DealerTakeTurnAsync();
    }

    // ==================== Explosion Effect ====================

    private async Task ShowExplosionAsync(bool targetIsDealer)
    {
        var btn = targetIsDealer ? BtnTopAction : BtnBottomAction;
        var savedText = GetEmojiText(btn);
        SetEmoji(btn, "\U0001F4A5");
        await Task.Delay(500);
        SetEmoji(btn, savedText);
    }

    // ==================== Shot Resolution ====================

    private async Task<bool> ResolveShotAsync(bool shooterIsPlayer, bool targetIsDealer)
    {
        if (_loaded.Count == 0) return false;

        bool isLive = _loaded[0];
        _loaded.RemoveAt(0);
        _dealerKnowsCurrent = null;

        // Update "Last Shell" display
        TxtLoaded.Text = isLive ? "\U0001F534" : "\U0001F535";

        // Play shot sound + pump 1s later
        Sfx.Play(isLive ? "shotlive.ogg" : "shotblank.ogg");
        _ = Sfx.PlayDelayedAsync("pump.ogg", 1000);

        // Stats: every shot is an action and ejects a shell
        _statGameActions++;
        _statGameShellsEjected++;

        AppendLog(Loc.LogShoot(shooterIsPlayer, targetIsDealer, isLive));

        int damage = _handsawActive ? 2 : 1;
        _handsawActive = false;

        if (isLive)
        {
            _statGameLiveHits++;

            if (targetIsDealer)
            {
                _dealerCharges = Math.Max(0, _dealerCharges - damage);
                AppendLog(Loc.LogTakeDamage(true, damage, _dealerCharges));
            }
            else
            {
                _playerCharges = Math.Max(0, _playerCharges - damage);
                AppendLog(Loc.LogTakeDamage(false, damage, _playerCharges));
            }
            UpdateChargesUI();

            await ShowExplosionAsync(targetIsDealer);

            if (_dealerCharges <= 0)
            {
                AppendLog(Loc.LogDealerDown);
                await Task.Delay(2000);
                await EndRoundAsync();
                return true;
            }
            if (_playerCharges <= 0)
            {
                EndGame();
                return true;
            }
        }

        // Extra pause after shot before flow continues
        await Task.Delay(1000);

        if (_loaded.Count == 0)
        {
            AppendLog(Loc.LogChamberEmpty);
            await Task.Delay(2000);
            await GenerateAndShowLoadedAsync();
            return true;
        }

        return false;
    }

    // ==================== Player Item Usage ====================

    private async Task UsePlayerItemAsync(int index)
    {
        ItemType item = _playerItems[index];
        if (item == ItemType.None) return;

        _playerItems[index] = ItemType.None;
        UpdateSlotUI(_playerSlotButtons, _playerItems, index);

        await ExecuteItemAsync(item, isPlayer: true);
    }

    private async Task UseAdrenalineStolenItemAsync(int dealerIndex)
    {
        ItemType item = _dealerItems[dealerIndex];
        if (item == ItemType.None || item == ItemType.Adrenaline) return;

        _dealerItems[dealerIndex] = ItemType.None;
        UpdateSlotUI(_dealerSlotButtons, _dealerItems, dealerIndex);

        AppendLog(Loc.LogStealFromDealer(item));
        await ExecuteItemAsync(item, isPlayer: true);
    }

    private async Task ExecuteItemAsync(ItemType item, bool isPlayer)
    {
        // Play item sound effect
        if (ItemSoundFile.TryGetValue(item, out var soundFile))
            Sfx.Play(soundFile);

        switch (item)
        {
            case ItemType.Handsaw:
                _handsawUsedThisCycle = true;
                if (_handsawActive)
                    AppendLog(Loc.LogUsedHandsawWasted(isPlayer));
                else
                {
                    _handsawActive = true;
                    AppendLog(Loc.LogUsedHandsaw(isPlayer));
                }
                break;

            case ItemType.Medicine:
                if (_rng.Next(2) == 0)
                {
                    int target = isPlayer ? _playerCharges : _dealerCharges;
                    int healed = Math.Min(target + 2, _maxCharges);
                    int gained = healed - target;
                    if (isPlayer) _playerCharges = healed;
                    else _dealerCharges = healed;
                    AppendLog(Loc.LogUsedMedicineHeal(isPlayer, gained));
                }
                else
                {
                    if (isPlayer) _playerCharges = Math.Max(0, _playerCharges - 1);
                    else _dealerCharges = Math.Max(0, _dealerCharges - 1);
                    AppendLog(Loc.LogUsedMedicineLose(isPlayer));

                    if ((isPlayer && _playerCharges <= 0) || (!isPlayer && _dealerCharges <= 0))
                    {
                        UpdateChargesUI();
                        if (isPlayer) { EndGame(); return; }
                        else { AppendLog(Loc.LogDealerDown); await Task.Delay(500); await EndRoundAsync(); return; }
                    }
                }
                UpdateChargesUI();
                break;

            case ItemType.Phone:
                if (_loaded.Count <= 1)
                    AppendLog(Loc.LogUsedPhoneWasted(isPlayer));
                else
                {
                    int idx = _rng.Next(1, _loaded.Count);
                    if (isPlayer)
                        AppendLog(Loc.LogUsedPhoneReveal(true, idx + 1, _loaded[idx]));
                    else
                        AppendLog(Loc.LogUsedPhoneDealerSecret);
                }
                break;

            case ItemType.Beer:
                if (_loaded.Count == 0)
                    AppendLog(Loc.LogUsedBeerWasted(isPlayer));
                else
                {
                    bool ejected = _loaded[0];
                    _loaded.RemoveAt(0);
                    _dealerKnowsCurrent = null;
                    TxtLoaded.Text = ejected ? "\U0001F534" : "\U0001F535";
                    AppendLog(Loc.LogUsedBeer(isPlayer, ejected));

                    _bonus = Math.Max(0, _bonus - 495);
                    _statGameShellsEjected++;
                    if (isPlayer) _statGameBeersPlayer++;

                    _ = Sfx.PlayDelayedAsync("pump.ogg", 1000);

                    if (_loaded.Count == 0)
                    {
                        AppendLog(Loc.LogChamberEmpty);
                        await Task.Delay(500);
                        await GenerateAndShowLoadedAsync();
                        return;
                    }
                }
                break;

            case ItemType.Cigarette:
                {
                    int before = isPlayer ? _playerCharges : _dealerCharges;
                    int after = Math.Min(before + 1, _maxCharges);
                    if (isPlayer) _playerCharges = after;
                    else _dealerCharges = after;
                    AppendLog(Loc.LogUsedCigarette(isPlayer, after - before));
                    UpdateChargesUI();

                    _bonus = Math.Max(0, _bonus - 220);
                    if (isPlayer) _statGameCigarettesPlayer++;
                }
                break;

            case ItemType.Magnifier:
                if (_loaded.Count == 0)
                    AppendLog(Loc.LogUsedMagnifierWasted(isPlayer));
                else
                {
                    bool current = _loaded[0];
                    AppendLog(Loc.LogUsedMagnifier(isPlayer, current));
                    if (!isPlayer) _dealerKnowsCurrent = current;
                }
                break;

            case ItemType.Inverter:
                if (_loaded.Count == 0)
                    AppendLog(Loc.LogUsedInverterWasted(isPlayer));
                else
                {
                    _loaded[0] = !_loaded[0];
                    AppendLog(Loc.LogUsedInverter(isPlayer));

                    if (!isPlayer)
                    {
                        if (_dealerKnowsCurrent.HasValue)
                            _dealerKnowsCurrent = !_dealerKnowsCurrent.Value;
                    }
                }
                break;

            case ItemType.Adrenaline:
                if (isPlayer)
                {
                    bool hasUsable = _dealerItems.Any(i => i != ItemType.None && i != ItemType.Adrenaline);
                    if (!hasUsable)
                    {
                        AppendLog(Loc.LogUsedAdrenalinePlayerWasted);
                        _adrenalineMode = false;
                    }
                    else
                    {
                        AppendLog(Loc.LogUsedAdrenalinePick);
                        _adrenalineMode = true;
                        return;
                    }
                }
                else
                {
                    await DealerUseAdrenalineAsync();
                }
                break;

            case ItemType.Handcuffs:
                if (_handcuffsUsedThisCycle)
                    AppendLog(Loc.LogUsedHandcuffsCycleWasted(isPlayer));
                else if (_handcuffsActive)
                    AppendLog(Loc.LogUsedHandcuffsActiveWasted(isPlayer));
                else
                {
                    _handcuffsActive = true;
                    _handcuffsUsedThisCycle = true;
                    AppendLog(Loc.LogUsedHandcuffs(isPlayer));
                }
                break;
        }

        await Task.Delay(300);
    }

    private async Task DealerUseAdrenalineAsync()
    {
        ItemType[] priority = { ItemType.Magnifier, ItemType.Handsaw, ItemType.Handcuffs, ItemType.Inverter,
                                ItemType.Beer, ItemType.Cigarette, ItemType.Medicine, ItemType.Phone };
        int bestIdx = -1;
        ItemType bestItem = ItemType.None;

        foreach (var desired in priority)
        {
            for (int i = 0; i < 8; i++)
            {
                if (_playerItems[i] == desired)
                {
                    bestIdx = i;
                    bestItem = desired;
                    goto found;
                }
            }
        }
        found:

        if (bestIdx < 0)
        {
            AppendLog(Loc.LogUsedAdrenalineDealerWasted);
            return;
        }

        _playerItems[bestIdx] = ItemType.None;
        UpdateSlotUI(_playerSlotButtons, _playerItems, bestIdx);
        AppendLog(Loc.LogDealerSteals(bestItem));
        await ExecuteItemAsync(bestItem, isPlayer: false);
    }

    // ==================== Dealer AI ====================

    private async Task DealerTakeTurnAsync()
    {
        Sfx.Play(_rng.Next(2) == 0 ? "pickgun1.ogg" : "pickgun2.ogg");
        await Task.Delay(1500);

        while (_phase == GamePhase.DealerTurn)
        {
            if (_loaded.Count == 0)
            {
                AppendLog(Loc.LogChamberEmpty);
                await Task.Delay(800);
                await GenerateAndShowLoadedAsync();
                return;
            }

            await DealerUseItemsAsync();

            if (_phase != GamePhase.DealerTurn) return;
            if (_loaded.Count == 0) continue;

            bool? known = _dealerKnowsCurrent;
            bool shootSelf;

            if (known == true) shootSelf = false;
            else if (known == false) shootSelf = true;
            else
            {
                int live = _loaded.Count(x => x);
                int blank = _loaded.Count(x => !x);
                if (blank > live) shootSelf = true;
                else if (live > blank) shootSelf = false;
                else shootSelf = _rng.Next(2) == 0;
            }

            // Handsaw: never risk double damage on self
            if (_handsawActive && shootSelf)
                shootSelf = false;

            bool targetIsDealer = shootSelf;
            AppendLog(Loc.LogDealerAims(targetIsDealer));
            await Task.Delay(1200);

            bool isLive = _loaded[0];
            bool stopped = await ResolveShotAsync(shooterIsPlayer: false, targetIsDealer: targetIsDealer);

            if (stopped) return;

            if (shootSelf && !isLive)
            {
                AppendLog(Loc.LogExtraTurnDealer);
                _handsawActive = false;
                _dealerKnowsCurrent = null;
                await Task.Delay(1000);
                continue;
            }

            await Task.Delay(800);
            if (_handcuffsActive)
            {
                _handcuffsActive = false;
                AppendLog(Loc.LogHandcuffedYou);
                _handsawActive = false;
                _dealerKnowsCurrent = null;
                _handcuffsUsedThisCycle = false;
                await Task.Delay(1000);
                AppendLog(Loc.LogDealerTurn);
                continue;
            }
            _handcuffsUsedThisCycle = false;
            SetPlayerTurn();
            return;
        }
    }

    private async Task DealerUseItemsAsync()
    {
        int itemsUsed = 0;
        const int maxItemsPerTurn = 4;

        async Task<bool> TryUse(ItemType type)
        {
            for (int i = 0; i < 8; i++)
            {
                if (_dealerItems[i] == type)
                {
                    _dealerItems[i] = ItemType.None;
                    UpdateSlotUI(_dealerSlotButtons, _dealerItems, i);
                    await ExecuteItemAsync(type, isPlayer: false);
                    itemsUsed++;
                    await Task.Delay(1300);
                    return true;
                }
            }
            return false;
        }

        bool HasItem(ItemType t) => _dealerItems.Any(i => i == t);

        if (_dealerCharges <= 2 && HasItem(ItemType.Cigarette))
            await TryUse(ItemType.Cigarette);
        if (_dealerCharges <= 1 && HasItem(ItemType.Medicine) && itemsUsed < maxItemsPerTurn)
            await TryUse(ItemType.Medicine);

        if (_phase != GamePhase.DealerTurn || _loaded.Count == 0) return;

        if (_dealerKnowsCurrent == null && HasItem(ItemType.Magnifier) && itemsUsed < maxItemsPerTurn)
            await TryUse(ItemType.Magnifier);

        if (_phase != GamePhase.DealerTurn || _loaded.Count == 0) return;

        if (_dealerKnowsCurrent == null && HasItem(ItemType.Phone) && itemsUsed < maxItemsPerTurn)
            await TryUse(ItemType.Phone);

        if (_phase != GamePhase.DealerTurn || _loaded.Count == 0) return;

        if (_dealerKnowsCurrent == false)
        {
            // Only try inverter; blank shell → dealer will shoot self for extra turn instead of wasting beer
            if (HasItem(ItemType.Inverter) && itemsUsed < maxItemsPerTurn)
                await TryUse(ItemType.Inverter);
        }

        if (_dealerKnowsCurrent == true)
        {
            if (!_handsawActive && !_handsawUsedThisCycle && HasItem(ItemType.Handsaw) && itemsUsed < maxItemsPerTurn)
                await TryUse(ItemType.Handsaw);
            if (!_handcuffsActive && !_handcuffsUsedThisCycle && HasItem(ItemType.Handcuffs) && itemsUsed < maxItemsPerTurn)
                await TryUse(ItemType.Handcuffs);
        }

        if (HasItem(ItemType.Adrenaline) && itemsUsed < maxItemsPerTurn)
        {
            bool playerHasGood = _playerItems.Any(i => i != ItemType.None && i != ItemType.Adrenaline);
            if (playerHasGood)
                await TryUse(ItemType.Adrenaline);
        }
    }

    // ==================== Round / Game Progression ====================

    private async Task EndRoundAsync()
    {
        _bonusTimer?.Stop();
        _roundCount++;

        if (_roundCount > 3)
        {
            // === GAME complete ===
            _money += _bonus;
            TxtMoneyNum.Text = $"{_money}$";

            AppendLog(Loc.LogGameComplete(_gameCount, _bonus, _money));

            Sfx.Play("win.ogg");

            bool playAgain = await ShowGameCompleteDialogAsync();

            // Accumulate to lifetime totals
            _statTotalActions += _statGameActions;
            _statTotalLiveHits += _statGameLiveHits;
            _statTotalShellsEjected += _statGameShellsEjected;
            _statTotalCigarettesPlayer += _statGameCigarettesPlayer;
            _statTotalBeersPlayer += _statGameBeersPlayer;

            if (playAgain)
            {
                _gameCount++;
                _roundCount = 1;
                _bonus = 70000;
                ResetPerGameStats();
                ClearAllItems();
                UpdateCounters();
                AppendLog(Loc.LogRoundOfGame(1, _gameCount));
                StartRound();
            }
            else
            {
                ResetGame();
                UpdateUILanguage();
            }
            return;
        }

        UpdateCounters();
        TxtMoneyNum.Text = $"{_money}$";
        ClearAllItems();

        AppendLog(Loc.LogRoundOfGame(_roundCount, _gameCount));
        await Task.Delay(1000);
        StartRound();
    }

    private async void EndGame()
    {
        _phase = GamePhase.GameOver;
        _bonusTimer?.Stop();
        BtnCenter.Appearance = ControlAppearance.Secondary;
        SetTargetButtonsAccent(false);
        TxtTurnInfo.Text = Loc.TurnGameOver;
        AppendLog(Loc.LogEliminated(_money));

        Sfx.Play("lose.ogg");
        await Task.Delay(300);
        await ShowDefeatDialogAsync();
        ResetGame();
        UpdateUILanguage();
    }

    private async Task ShowDefeatDialogAsync()
    {
        // Accumulate stats for display
        int totalActions = _statTotalActions + _statGameActions;
        int totalLiveHits = _statTotalLiveHits + _statGameLiveHits;
        int totalShells = _statTotalShellsEjected + _statGameShellsEjected;
        int totalCigs = _statTotalCigarettesPlayer + _statGameCigarettesPlayer;
        int totalBeers = _statTotalBeersPlayer + _statGameBeersPlayer;

        var content = new StackPanel { Margin = new Thickness(8, 4, 8, 8) };

        // Stats table
        var grid = new Grid { Margin = new Thickness(0, 8, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

        int row = 0;
        AddStatsRow(grid, row++, "", Loc.StatsThisGame, Loc.StatsTotal, true);
        AddStatsRow(grid, row++, Loc.StatsActions, _statGameActions.ToString(), totalActions.ToString(), false);
        AddStatsRow(grid, row++, Loc.StatsEffectiveShots, _statGameLiveHits.ToString(), totalLiveHits.ToString(), false);
        AddStatsRow(grid, row++, Loc.StatsShellsEjected, _statGameShellsEjected.ToString(), totalShells.ToString(), false);
        AddStatsRow(grid, row++, Loc.StatsCigarettes, _statGameCigarettesPlayer.ToString(), totalCigs.ToString(), false);
        AddStatsRow(grid, row++, Loc.StatsBeer, (_statGameBeersPlayer * 330).ToString(), (totalBeers * 330).ToString(), false);

        content.Children.Add(grid);

        // Cash section
        var cashPanel = new StackPanel { Margin = new Thickness(0, 16, 0, 0) };
        cashPanel.Children.Add(new TextBlock
        {
            Text = Loc.StatsCashEarned,
            FontSize = 12,
            Opacity = 0.6
        });
        cashPanel.Children.Add(new TextBlock
        {
            Text = $"{_money}$",
            FontSize = 28,
            FontWeight = FontWeights.Bold
        });
        content.Children.Add(cashPanel);

        // Defeat story text
        content.Children.Add(new TextBlock
        {
            Text = Loc.StatsDefeatStory,
            FontSize = 12,
            Opacity = 0.6,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0)
        });

        var dialog = new Wpf.Ui.Controls.MessageBox
        {
            Title = Loc.StatsDefeatTitle,
            Content = content,
            CloseButtonText = Loc.StatsDefeatAccept
        };

        await dialog.ShowDialogAsync();
    }

    // ==================== Stats Dialog ====================

    private async Task<bool> ShowGameCompleteDialogAsync()
    {
        var content = new StackPanel { Margin = new Thickness(8, 4, 8, 8) };

        // Stats table
        var grid = new Grid { Margin = new Thickness(0, 8, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

        int row = 0;

        int totalActions = _statTotalActions + _statGameActions;
        int totalLiveHits = _statTotalLiveHits + _statGameLiveHits;
        int totalShells = _statTotalShellsEjected + _statGameShellsEjected;
        int totalCigs = _statTotalCigarettesPlayer + _statGameCigarettesPlayer;
        int totalBeers = _statTotalBeersPlayer + _statGameBeersPlayer;

        AddStatsRow(grid, row++, "", Loc.StatsThisGame, Loc.StatsTotal, true);
        AddStatsRow(grid, row++, Loc.StatsActions, _statGameActions.ToString(), totalActions.ToString(), false);
        AddStatsRow(grid, row++, Loc.StatsEffectiveShots, _statGameLiveHits.ToString(), totalLiveHits.ToString(), false);
        AddStatsRow(grid, row++, Loc.StatsShellsEjected, _statGameShellsEjected.ToString(), totalShells.ToString(), false);
        AddStatsRow(grid, row++, Loc.StatsCigarettes, _statGameCigarettesPlayer.ToString(), totalCigs.ToString(), false);
        AddStatsRow(grid, row++, Loc.StatsBeer, (_statGameBeersPlayer * 330).ToString(), (totalBeers * 330).ToString(), false);

        content.Children.Add(grid);

        // Cash earned
        var cashPanel = new StackPanel { Margin = new Thickness(0, 16, 0, 0) };
        cashPanel.Children.Add(new TextBlock
        {
            Text = Loc.StatsCashEarned,
            FontSize = 12,
            Opacity = 0.6
        });
        cashPanel.Children.Add(new TextBlock
        {
            Text = $"{_bonus}$",
            FontSize = 28,
            FontWeight = FontWeights.Bold
        });
        content.Children.Add(cashPanel);

        // Story text
        content.Children.Add(new TextBlock
        {
            Text = Loc.StatsWinStory,
            FontSize = 12,
            Opacity = 0.6,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0)
        });

        var dialog = new Wpf.Ui.Controls.MessageBox
        {
            Title = Loc.StatsTitle,
            Content = content,
            PrimaryButtonText = Loc.StatsPlayAgain,
            CloseButtonText = Loc.StatsQuit
        };

        var result = await dialog.ShowDialogAsync();
        return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
    }

    private static void AddStatsRow(Grid grid, int row, string label, string thisGame, string total, bool isHeader)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var weight = isHeader ? FontWeights.SemiBold : FontWeights.Normal;
        var size = isHeader ? 12.0 : 13.0;

        var lblBlock = new TextBlock
        {
            Text = label,
            FontSize = size,
            FontWeight = weight,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3, 12, 3)
        };
        Grid.SetRow(lblBlock, row);
        Grid.SetColumn(lblBlock, 0);
        grid.Children.Add(lblBlock);

        var gameBlock = new TextBlock
        {
            Text = thisGame,
            FontSize = size,
            FontWeight = weight,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3, 0, 3)
        };
        Grid.SetRow(gameBlock, row);
        Grid.SetColumn(gameBlock, 1);
        grid.Children.Add(gameBlock);

        var totalBlock = new TextBlock
        {
            Text = total,
            FontSize = size,
            FontWeight = weight,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3, 0, 3)
        };
        Grid.SetRow(totalBlock, row);
        Grid.SetColumn(totalBlock, 2);
        grid.Children.Add(totalBlock);
    }

    // ==================== UI Helpers ====================

    private void UpdateCounters()
    {
        TxtGameNum.Text = _gameCount.ToString();
        TxtRoundNum.Text = _roundCount.ToString();
    }

    private void UpdateChargesUI()
    {
        TxtChargeDealer.Text = new string('\u26A1', _dealerCharges);
        TxtChargePlayer.Text = new string('\u26A1', _playerCharges);
    }

    private static string ShellsToString(List<bool> shells)
    {
        return string.Concat(shells.Select(s => s ? "\U0001F534" : "\U0001F535"));
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void AppendLog(string message)
    {
        if (string.IsNullOrEmpty(TxtLog.Text))
            TxtLog.Text = message;
        else
            TxtLog.Text += "\r\n" + message;

        ScrollLogToBottom();
    }

    private void ScrollLogToBottom()
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
        {
            var sv = FindChildScrollViewer(TxtLog);
            sv?.ScrollToEnd();
        });
    }

    private static ScrollViewer? FindChildScrollViewer(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ScrollViewer sv) return sv;
            var result = FindChildScrollViewer(child);
            if (result != null) return result;
        }
        return null;
    }

    private void UpdateSlotUI(Button[] buttons, ItemType[] items, int index)
    {
        if (items[index] == ItemType.None)
            buttons[index].Content = null;
        else if (ItemEmoji.TryGetValue(items[index], out var emoji))
            SetEmoji(buttons[index], emoji, 20);
    }

    private void ClearAllItems()
    {
        for (int i = 0; i < 8; i++)
        {
            _dealerItems[i] = ItemType.None;
            _playerItems[i] = ItemType.None;
            _dealerSlotButtons[i].Content = null;
            _playerSlotButtons[i].Content = null;
        }
    }

    private (bool isDealer, int index) IdentifySlot(Button btn)
    {
        for (int i = 0; i < 8; i++)
        {
            if (_dealerSlotButtons[i] == btn) return (true, i);
            if (_playerSlotButtons[i] == btn) return (false, i);
        }
        return (false, -1);
    }

    // ==================== Emoji Helpers ====================

    private static void SetEmoji(ContentControl ctl, string text, double fontSize = 30)
    {
        if (ctl.Content is Emoji.Wpf.TextBlock tb)
        {
            tb.Text = text;
            tb.FontSize = fontSize;
        }
        else
        {
            ctl.Content = new Emoji.Wpf.TextBlock
            {
                Text = text,
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }
    }

    private static string GetEmojiText(ContentControl ctl)
    {
        if (ctl.Content is Emoji.Wpf.TextBlock tb) return tb.Text;
        return ctl.Content?.ToString() ?? "";
    }

    // ==================== Theme ====================

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        ApplicationTheme theme;
        if (_themeMode == "dark")
            theme = ApplicationTheme.Dark;
        else if (_themeMode == "light")
            theme = ApplicationTheme.Light;
        else
        {
            var sys = ApplicationThemeManager.GetSystemTheme();
            theme = sys == SystemTheme.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }

        ApplicationThemeManager.Apply(theme, Wpf.Ui.Controls.WindowBackdropType.Mica);

        if (!IsLoaded)
            return;

        if (_themeMode == "system")
        {
            if (!_isWatchingSystemTheme)
            {
                SystemThemeWatcher.Watch(this, Wpf.Ui.Controls.WindowBackdropType.Mica);
                _isWatchingSystemTheme = true;
            }
        }
        else if (_isWatchingSystemTheme)
        {
            SystemThemeWatcher.UnWatch(this);
            _isWatchingSystemTheme = false;
        }
    }

    private void ThemeSystem_Click(object sender, RoutedEventArgs e)
    {
        _themeMode = "system";
        MenuThemeSystem.IsChecked = true;
        MenuThemeDark.IsChecked = false;
        MenuThemeLight.IsChecked = false;
        ApplyTheme();
    }

    private void ThemeDark_Click(object sender, RoutedEventArgs e)
    {
        _themeMode = "dark";
        MenuThemeSystem.IsChecked = false;
        MenuThemeDark.IsChecked = true;
        MenuThemeLight.IsChecked = false;
        ApplyTheme();
    }

    private void ThemeLight_Click(object sender, RoutedEventArgs e)
    {
        _themeMode = "light";
        MenuThemeSystem.IsChecked = false;
        MenuThemeDark.IsChecked = false;
        MenuThemeLight.IsChecked = true;
        ApplyTheme();
    }

    // ==================== Bonus Timer ====================

    private void BonusTimer_Tick(object? sender, EventArgs e)
    {
        _bonus = Math.Max(0, _bonus - 200);
    }

    // ==================== UI Events: Slot Buttons ====================

    private void BtnSlot_Click(object sender, RoutedEventArgs e)
    {
        var (isDealer, index) = IdentifySlot((Button)sender);
        if (index < 0) return;

        if (_adrenalineMode && isDealer)
        {
            ItemType item = _dealerItems[index];
            if (item == ItemType.None || item == ItemType.Adrenaline) return;
            _adrenalineMode = false;
            _ = UseAdrenalineStolenItemAsync(index);
            return;
        }

        if (!isDealer && _phase == GamePhase.PlayerTurnIdle)
        {
            ItemType item = _playerItems[index];
            if (item == ItemType.None) return;
        if (item == ItemType.Handsaw && (_handsawActive || _handsawUsedThisCycle))
        {
            TxtHint.Text = Loc.HintHandsawBlocked;
            return;
        }
        if (item == ItemType.Handcuffs && (_handcuffsUsedThisCycle || _handcuffsActive))
        {
            TxtHint.Text = Loc.HintHandcuffsBlocked;
            return;
        }

        _ = UsePlayerItemAsync(index);
        }
    }

    private void BtnSlot_MouseEnter(object sender, MouseEventArgs e)
    {
        var (isDealer, index) = IdentifySlot((Button)sender);
        if (index < 0) return;

        ItemType item = isDealer ? _dealerItems[index] : _playerItems[index];
        if (item != ItemType.None)
        {
            string hint = Loc.GetItemHint(item);
            if (!string.IsNullOrEmpty(hint))
                TxtHint.Text = hint;
        }
    }

    private void BtnSlot_MouseLeave(object sender, MouseEventArgs e)
    {
        TxtHint.Text = "";
    }

    // ==================== UI Events: Center Button ====================

    private void BtnCenter_Click(object sender, RoutedEventArgs e)
    {
        switch (_phase)
        {
            case GamePhase.Idle:
                StartGame();
                break;
            case GamePhase.PlayerTurnIdle:
                _phase = GamePhase.PlayerTurnArmed;
                Sfx.Play(_rng.Next(2) == 0 ? "pickgun1.ogg" : "pickgun2.ogg");
                // Gun becomes subtle, targets become accent
                BtnCenter.Appearance = ControlAppearance.Secondary;
                SetTargetButtonsAccent(true);
                TxtHint.Text = Loc.HintChooseTarget;
                AppendLog(Loc.LogPickUpGun);
                break;
        }
    }

    private void BtnCenter_MouseEnter(object sender, MouseEventArgs e)
    {
        if (_phase == GamePhase.Idle)
            TxtHint.Text = Loc.HintRiskLife;
        else if (_phase == GamePhase.PlayerTurnIdle)
            TxtHint.Text = Loc.HintAfraid;
    }

    private void BtnCenter_MouseLeave(object sender, MouseEventArgs e)
    {
        if (_phase != GamePhase.PlayerTurnArmed)
            TxtHint.Text = "";
    }

    // ==================== UI Events: Top Button (Dealer) ====================

    private void BtnTopAction_Click(object sender, RoutedEventArgs e)
    {
        if (_phase != GamePhase.PlayerTurnArmed) return;
        TxtHint.Text = "";
        _ = PlayerShootAsync(targetIsDealer: true);
    }

    private void BtnTopAction_MouseEnter(object sender, MouseEventArgs e)
    {
        TxtHint.Text = Loc.HintDealer;
    }

    private void BtnTopAction_MouseLeave(object sender, MouseEventArgs e)
    {
        if (_phase == GamePhase.PlayerTurnArmed)
            TxtHint.Text = Loc.HintChooseTarget;
        else
            TxtHint.Text = "";
    }

    // ==================== UI Events: Bottom Button (You) ====================

    private void BtnBottomAction_Click(object sender, RoutedEventArgs e)
    {
        if (_phase != GamePhase.PlayerTurnArmed) return;
        TxtHint.Text = "";
        _ = PlayerShootAsync(targetIsDealer: false);
    }

    private void BtnBottomAction_MouseEnter(object sender, MouseEventArgs e)
    {
        TxtHint.Text = Loc.HintYou;
    }

    private void BtnBottomAction_MouseLeave(object sender, MouseEventArgs e)
    {
        if (_phase == GamePhase.PlayerTurnArmed)
            TxtHint.Text = Loc.HintChooseTarget;
        else
            TxtHint.Text = "";
    }

    // ==================== Player Shoot ====================

    private async Task PlayerShootAsync(bool targetIsDealer)
    {
        _phase = GamePhase.ShowingLoaded;

        // Revert button styles after target selection
        BtnCenter.Appearance = ControlAppearance.Secondary;
        SetTargetButtonsAccent(false);

        bool isLive = _loaded.Count > 0 && _loaded[0];
        bool stopped = await ResolveShotAsync(shooterIsPlayer: true, targetIsDealer: targetIsDealer);

        if (stopped) return;

        if (!targetIsDealer && !isLive)
        {
            AppendLog(Loc.LogExtraTurnYou);
            _phase = GamePhase.PlayerTurnIdle;
            _handsawActive = false;
            TxtTurnInfo.Text = Loc.TurnYour;
            BtnCenter.Appearance = ControlAppearance.Primary;
            _bonusTimer?.Start();
            return;
        }

        if (_handcuffsActive)
        {
            _handcuffsActive = false;
            AppendLog(Loc.LogHandcuffedDealer);
            _handcuffsUsedThisCycle = false;
            await Task.Delay(600);
            SetPlayerTurn();
            return;
        }

        _handcuffsUsedThisCycle = false;
        SetDealerTurn();
    }

    // ==================== Menu Events ====================

    private void NewGame_Click(object sender, RoutedEventArgs e)
    {
        ResetGame();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void About_Click(object sender, RoutedEventArgs e)
    {
        var content = new StackPanel
        {
            Width = 396,
            Margin = new Thickness(8, 4, 8, 8)
        };

        content.Children.Add(new TextBlock
        {
            Text = "Buckshot Fluentte",
            FontSize = 22,
            FontWeight = FontWeights.SemiBold
        });
        content.Children.Add(new TextBlock
        {
            Text = Loc.DialogAboutVersion,
            FontSize = 12,
            Opacity = 0.6,
            Margin = new Thickness(0, 2, 0, 2)
        });
        content.Children.Add(new TextBlock
        {
            Text = Loc.DialogAboutCredit,
            FontSize = 12,
            Opacity = 0.7,
            Margin = new Thickness(0, 0, 0, 12)
        });
        content.Children.Add(new TextBlock
        {
            Text = Loc.DialogAboutDescription,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 16)
        });

        var warningContent = new StackPanel();
        warningContent.Children.Add(new TextBlock
        {
            Text = Loc.DialogAboutWarningTitle,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 6)
        });
        warningContent.Children.Add(new TextBlock
        {
            Text = Loc.DialogAboutWarningContent,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 22
        });

        var warningCard = new Border
        {
            Padding = new Thickness(12, 10, 12, 10),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = warningContent,
            Margin = new Thickness(0, 0, 0, 16)
        };
        warningCard.SetResourceReference(Border.BackgroundProperty, "ControlFillColorDefaultBrush");
        warningCard.SetResourceReference(Border.BorderBrushProperty, "ControlStrokeColorDefaultBrush");
        content.Children.Add(warningCard);

        content.Children.Add(new TextBlock
        {
            Text = Loc.DialogAboutTribute,
            FontSize = 12,
            Opacity = 0.72,
            TextWrapping = TextWrapping.Wrap
        });

        var dialog = new Wpf.Ui.Controls.MessageBox
        {
            Title = Loc.DialogAboutTitle,
            Content = content,
            CloseButtonText = Loc.DialogOK
        };
        await dialog.ShowDialogAsync();
    }

    private void LangEn_Click(object sender, RoutedEventArgs e)
    {
        Loc.Language = "en";
        UpdateLanguageMenuSelection();
        UpdateUILanguage();
    }

    private void LangZh_Click(object sender, RoutedEventArgs e)
    {
        Loc.Language = "zh";
        UpdateLanguageMenuSelection();
        UpdateUILanguage();
    }
}
