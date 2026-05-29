namespace BuckshotFluentte;

public static class Loc
{
    private static string _lang = "en";
    public static string Language
    {
        get => _lang;
        set => _lang = value;
    }

    private static string L(string en, string zh) => _lang == "zh" ? zh : en;

    // ===== Menu =====
    public static string MenuGame => L("Game", "游戏");
    public static string MenuNewGame => L("New Game", "新游戏");
    public static string MenuClose => L("Close", "关闭");
    public static string MenuSettings => L("Settings", "设置");
    public static string MenuLanguage => L("Language", "语言");
    public static string MenuAbout => L("About", "关于");
    public static string MenuTheme => L("Theme", "主题");
    public static string ThemeSystem => L("Follow System", "跟随系统");
    public static string ThemeDark => L("Dark", "深色");
    public static string ThemeLight => L("Light", "浅色");

    // ===== Labels =====
    public static string LabelGame => L("GAME", "\u573A");
    public static string LabelRound => L("ROUND", "\u8F6E");
    public static string LabelMoney => L("MONEY", "金额");
    public static string LabelCharges => L("Charges:", "生命值:");
    public static string LabelDealer => L("Dealer:", "庄家:");
    public static string LabelPlayer => L("Player:", "玩家:");
    public static string LabelLoaded => L("Loaded:", "载弹:");
    public static string LabelLastShell => L("Last Shell:", "上一发:");
    public static string LastShellAwaiting => L("Awaiting fire", "等待发射");
    public static string LabelActionLog => L("Action Log", "行动记录");

    // ===== Turn Info =====
    public static string TurnAwaiting => L("Awaiting the game...", "等待游戏开始...");
    public static string TurnYour => L("Your turn", "你的回合");
    public static string TurnDealer => L("Dealer's turn", "庄家的回合");
    public static string TurnLoading => L("Loading shells...", "装弹中...");
    public static string TurnGameOver => L("Game Over", "游戏结束");

    // ===== Hints =====
    public static string HintRiskLife => L("Risk your life.", "用你的命来赌。");
    public static string HintAfraid => L("Afraid? How unfortunate.", "害怕了？太不幸了。");
    public static string HintChooseTarget => L("Choose your target.", "选择你的目标。");
    public static string HintDealer => L("Dealer", "庄家");
    public static string HintYou => L("You", "你");
    public static string HintHandsawBlocked => L("Handsaw can only be used once per turn.", "手锯每回合只能使用一次。");
    public static string HintHandcuffsBlocked => L("Handcuffs cannot be used right now.", "手铐当前无法使用。");

    // ===== Item Hints =====
    public static string GetItemHint(ItemType type) => type switch
    {
        ItemType.Handsaw    => L("Handsaw: Saw off the barrel, double the damage.",
                                 "手锯：锯掉枪管，伤害翻倍。"),
        ItemType.Medicine   => L("Expired Medicine: Fifty-fifty chance, heal 2 charges or lose 1 charge.",
                                 "过期药品：五五开概率，恢复2点生命或失去1点生命。"),
        ItemType.Phone      => L("Burner Phone: A mysterious voice telling you about the future.",
                                 "一次性手机：一个神秘的声音告诉你未来。"),
        ItemType.Beer       => L("Beer: Unload the current shell from the chamber without firing it.",
                                 "啤酒：不开火的情况下退出一发弹药。"),
        ItemType.Cigarette  => L("Cigarette: Reduce the anxiety. Gain one charge.",
                                 "香烟：减轻焦虑。恢复1点生命值。"),
        ItemType.Magnifier  => L("Magnifier: Check the current shell in the chamber.",
                                 "放大镜：查看当前枪膛中的弹药。"),
        ItemType.Inverter   => L("Inverter: Change the lethality of the current shell in the chamber.",
                                 "逆转器：改变当前枪膛中弹药的杀伤性。"),
        ItemType.Adrenaline => L("Adrenaline: Take one item from opponent.",
                                 "肾上腺素：从对手处拿走一个道具。"),
        ItemType.Handcuffs  => L("Handcuffs: Prevent the opponent from taking their next turn.",
                                 "手铐：限制对手下一回合的行动。"),
        _ => ""
    };

    // ===== Item Names =====
    public static string ItemName(ItemType type) => type switch
    {
        ItemType.Handsaw    => L("Handsaw", "手锯"),
        ItemType.Medicine   => L("Medicine", "过期药品"),
        ItemType.Phone      => L("Burner Phone", "一次性手机"),
        ItemType.Beer       => L("Beer", "啤酒"),
        ItemType.Cigarette  => L("Cigarette", "香烟"),
        ItemType.Magnifier  => L("Magnifier", "放大镜"),
        ItemType.Inverter   => L("Inverter", "逆转器"),
        ItemType.Adrenaline => L("Adrenaline", "肾上腺素"),
        ItemType.Handcuffs  => L("Handcuffs", "手铐"),
        _ => ""
    };

    // ===== Names =====
    public static string NameYou => L("You", "你");
    public static string NameDealer => L("Dealer", "庄家");
    public static string UserName(bool isPlayer) => isPlayer ? NameYou : NameDealer;

    // ===== Log: Game Flow =====
    public static string LogSignWaiver => L("Please sign the waiver.", "请签署弃权书。");
    public static string LogGameStart => L("Game start.", "游戏开始。");
    public static string LogYourTurn => L("Your turn.", "你的回合。");
    public static string LogDealerTurn => L("Dealer's turn.", "庄家的回合。");
    public static string LogPickUpGun => L("You pick up the shotgun...", "你拿起了霰弹枪...");
    public static string LogDealerDown => L("Dealer is down!", "庄家倒下了！");
    public static string LogChamberEmpty => L("Chamber empty. Reloading...", "弹膛已空。重新装填...");
    public static string LogExtraTurnYou => L("You get another turn.", "你获得了额外回合。");
    public static string LogExtraTurnDealer => L("Dealer gets another turn.", "庄家获得了额外回合。");
    public static string LogHandcuffedYou => L("You are handcuffed! Turn skipped.", "你被铐住了！跳过回合。");
    public static string LogHandcuffedDealer => L("Dealer is handcuffed! Turn skipped.", "庄家被铐住了！跳过回合。");

    // ===== Log: Parameterized =====
    public static string LogRoundBegins(int round, int charges) =>
        L($"Round {round} begins. Charges: {charges}.", $"第{round}轮开始。生命值：{charges}。");

    public static string LogLoaded(int count, int live, int blank) =>
        L($"Loaded: {count} shells ({live} live, {blank} blank).",
          $"装填：{count}发弹药（{live}实弹，{blank}空弹）。");

    public static string LogShoot(bool shooterIsPlayer, bool targetIsDealer, bool isLive)
    {
        var shooter = shooterIsPlayer ? NameYou : NameDealer;
        var target = targetIsDealer ? NameDealer : NameYou;
        var shell = isLive ? L("LIVE", "实弹") : L("BLANK", "空弹");
        return $"{shooter}{L(" shoots ", "射击了")}{target}{L(". ", "。")}[{shell}]";
    }

    public static string LogTakeDamage(bool isDealer, int damage, int remaining) =>
        isDealer
            ? L($"Dealer takes {damage} damage! ({remaining} charges left)",
                $"庄家受到{damage}点伤害！（剩余{remaining}生命值）")
            : L($"You take {damage} damage! ({remaining} charges left)",
                $"你受到{damage}点伤害！（剩余{remaining}生命值）");

    public static string LogGameComplete(int game, int bonus, int totalMoney) =>
        L($"Game {game} complete! Bonus: {bonus}. Total money: {totalMoney}.",
          $"第{game}场完成！奖金：{bonus}。总金额：{totalMoney}。");

    public static string LogRoundOfGame(int round, int game) =>
        L($"Round {round} of Game {game} starting...",
          $"第{game}场第{round}轮开始...");

    public static string LogEliminated(int money) =>
        L($"You have been eliminated. Final money: {money}.",
          $"你已被淘汰。最终金额：{money}。");

    public static string LogDealerAims(bool atSelf) =>
        L($"Dealer aims at {(atSelf ? "self" : "You")}...",
          $"庄家瞄准了{(atSelf ? "自己" : "你")}...");

    // ===== Log: Item Usage =====
    public static string LogUsedHandsaw(bool p) =>
        L($"{UserName(p)} used Handsaw. Next shot deals double damage.",
          $"{UserName(p)}使用了手锯。下一发造成双倍伤害。");

    public static string LogUsedHandsawWasted(bool p) =>
        L($"{UserName(p)} used Handsaw, but it's already active. Wasted!",
          $"{UserName(p)}使用了手锯，但已经激活。浪费了！");

    public static string LogUsedMedicineHeal(bool p, int gained) =>
        L($"{UserName(p)} used Medicine. Healed {gained} charge(s).",
          $"{UserName(p)}使用了过期药品。恢复了{gained}点生命值。");

    public static string LogUsedMedicineLose(bool p) =>
        L($"{UserName(p)} used Medicine. Lost 1 charge!",
          $"{UserName(p)}使用了过期药品。失去了1点生命值！");

    public static string LogUsedPhoneReveal(bool p, int shellNum, bool isLive)
    {
        var type = isLive ? L("LIVE", "实弹") : L("BLANK", "空弹");
        return L($"{UserName(p)} used Burner Phone. Shell #{shellNum} is {type}.",
                 $"{UserName(p)}使用了一次性手机。第{shellNum}发是{type}。");
    }

    public static string LogUsedPhoneWasted(bool p) =>
        L($"{UserName(p)} used Burner Phone. No future shells to reveal. Wasted!",
          $"{UserName(p)}使用了一次性手机。没有可以透露的弹药。浪费了！");

    public static string LogUsedPhoneDealerSecret =>
        L("Dealer used Burner Phone. He seems to learn something...",
          "庄家使用了一次性手机。他似乎获知了某些信息...");

    public static string LogUsedBeer(bool p, bool wasLive)
    {
        var type = wasLive ? L("LIVE", "实弹") : L("BLANK", "空弹");
        return L($"{UserName(p)} used Beer. Ejected a {type} shell.",
                 $"{UserName(p)}使用了啤酒。退出了一发{type}。");
    }

    public static string LogUsedBeerWasted(bool p) =>
        L($"{UserName(p)} used Beer. Chamber already empty. Wasted!",
          $"{UserName(p)}使用了啤酒。弹膛已空。浪费了！");

    public static string LogUsedCigarette(bool p, int gained) =>
        L($"{UserName(p)} used Cigarette. +{gained} charge.",
          $"{UserName(p)}使用了香烟。+{gained}生命值。");

    public static string LogUsedMagnifier(bool p, bool isLive)
    {
        var type = isLive ? L("LIVE", "实弹") : L("BLANK", "空弹");
        return L($"{UserName(p)} used Magnifier. Current shell is {type}.",
                 $"{UserName(p)}使用了放大镜。当前弹药是{type}。");
    }

    public static string LogUsedMagnifierWasted(bool p) =>
        L($"{UserName(p)} used Magnifier. Chamber empty. Wasted!",
          $"{UserName(p)}使用了放大镜。弹膛已空。浪费了！");

    public static string LogUsedInverter(bool p) =>
        L($"{UserName(p)} used Inverter. The shell has been inverted.",
          $"{UserName(p)}使用了逆转器。弹药已被逆转。");

    public static string LogUsedInverterWasted(bool p) =>
        L($"{UserName(p)} used Inverter. Chamber empty. Wasted!",
          $"{UserName(p)}使用了逆转器。弹膛已空。浪费了！");

    public static string LogUsedAdrenalinePick =>
        L("You used Adrenaline. Pick an item from Dealer's grid.",
          "你使用了肾上腺素。从庄家的道具中选择一个。");

    public static string LogUsedAdrenalinePlayerWasted =>
        L("You used Adrenaline. Dealer has no stealable items. Wasted!",
          "你使用了肾上腺素。庄家没有可偷取的道具。浪费了！");

    public static string LogUsedAdrenalineDealerWasted =>
        L("Dealer used Adrenaline. You have no stealable items. Wasted!",
          "庄家使用了肾上腺素。你没有可偷取的道具。浪费了！");

    public static string LogStealFromDealer(ItemType item) =>
        L($"You steal and use Dealer's {ItemName(item)}!",
          $"你偷取并使用了庄家的{ItemName(item)}！");

    public static string LogDealerSteals(ItemType item) =>
        L($"Dealer steals your {ItemName(item)}!",
          $"庄家偷取了你的{ItemName(item)}！");

    public static string LogUsedHandcuffs(bool p) =>
        L($"{UserName(p)} used Handcuffs. Opponent's next turn will be skipped.",
          $"{UserName(p)}使用了手铐。对手的下一回合将被跳过。");

    public static string LogUsedHandcuffsCycleWasted(bool p) =>
        L($"{UserName(p)} tried to use Handcuffs, but already used this cycle. Wasted!",
          $"{UserName(p)}试图使用手铐，但本轮已使用过。浪费了！");

    public static string LogUsedHandcuffsActiveWasted(bool p) =>
        L($"{UserName(p)} tried to use Handcuffs, but already active. Wasted!",
          $"{UserName(p)}试图使用手铐，但已经激活。浪费了！");

    // ===== Dialogs =====
    public static string DialogGameOverTitle => L("Game Over", "游戏结束");
    public static string DialogGameOverContent(int games, int rounds, int money) =>
        L($"You survived {games} game(s), {rounds} round(s).\nFinal money: {money}.",
          $"你存活了{games}场{rounds}轮。\n最终金额：{money}。");
    public static string DialogOK => L("OK", "确定");
    public static string DialogAboutTitle => L("About Buckshot Fluentte", "关于 Buckshot Fluentte");
    public static string DialogAboutVersion => L("Version 1.0", "版本 1.0");
    public static string DialogAboutCredit => L("by Aeka", "\u7531Aeka\u5236\u4F5C");
    public static string DialogAboutDescription =>
        L("A modern retro game in Fluent style.", "一款 Fluent 风格的现代复古游戏。");
    public static string DialogAboutWarningTitle => L("Health Reminder", "健康提醒");
    public static string DialogAboutWarningContent =>
        L("Smoking and excessive drinking are harmful to your health.\nStay away from gambling.",
          "吸烟和过量饮酒有害健康。\n远离赌博。");
    public static string DialogAboutTribute =>
        L("This is a fan-made tribute. Consider trying the original Buckshot Roulette by Mike Klubnika!",
          "本作为同人致敬作品。推荐体验 Mike Klubnika 制作的原版恶魔轮盘（Buckshot Roulette）！");

    // ===== Stats Dialog =====
    public static string StatsTitle => L("Congratulations, you won!", "恭喜大佬，你赢了。");
    public static string StatsThisGame => L("This Game", "本场");
    public static string StatsTotal => L("Total", "总计");
    public static string StatsActions => L("Actions taken", "进行的回合数");
    public static string StatsEffectiveShots => L("Effective shots", "开火次数");
    public static string StatsShellsEjected => L("Shells ejected", "弹壳弹出数");
    public static string StatsCigarettes => L("Cigarettes (sticks)", "香烟消耗（根）");
    public static string StatsBeer => L("Beer consumed (ml)", "啤酒饮用（毫升）");
    public static string StatsCashEarned => L("Cash earned:", "赢得现金：");
    public static string StatsPlayAgain => L("Roll higher", "再干一票");
    public static string StatsQuit => L("Call it quits", "到此为止");
    public static string StatsWinStory =>
        L("The Dealer kept his word, a briefcase of cash was delivered to your hands. Now, will you call it quits, or seize the chance to get rich overnight?",
          "庄家没有食言，一公文包的钱送到了你的手中。现在，你要就此收手，还是抓住机会，一夜暴富？");

    // ===== Defeat Dialog =====
    public static string StatsDefeatTitle => L("Don't forget the agreement.", "别忘了一开始的协议。");
    public static string StatsDefeatStory =>
        L("You can beat Death countless times, but Death only needs to beat you once. No matter how much money you won before, you won't live to enjoy it.",
          "你可以赢死神无数次，而死神只要赢你一次。无论你之前赢了多少钱，你都无命享用了。");
    public static string StatsDefeatAccept => L("Accept the loss", "愿赌服输");
}
