using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public const int MAX_PLAYERS = 18;
    public const int MAX_CREEPS = 100;
    public const int MAX_PERKS = 8;
    public const int MAX_STAGE = 4;
    public const int MAX_UPGRADE_LVL = 50;
    public const int MIN_RUNE_SPAWN_DELAY = 100;
    public const int MAX_RUNE_SPAWN_DELAY = 170;
    public const int RUNE_DURATION = 30;

    public static List<AIProfile> aiProfiles = new()
    {
        new() { name = "Васявик" },
        new() { name = "Гачи" },
        new() { name = "Mega Player" },
        new() { name = "Армен" },
        new() { name = "Тень" },
        new() { name = "Shadow Master" },
        new() { name = "Smooth Blade" },
        new() { name = "Cowmarine" },
        new() { name = "Кто здесь?" },
        new() { name = "Капуста" },
        new() { name = "Светильник" },
        new() { name = "Mandalorian" },
        new() { name = "Сумчатый" },
        new() { name = "Spotted lynx" },
        new() { name = "Рысь Пятнистая" },
        new() { name = "Ем Зубную пасту" },
        new() { name = "plunger" },
        new() { name = "-=O_o=-" },
        new() { name = "Восэм хромосом" },
        new() { name = "Toilet bowl - my friend" },
        new() { name = "Дружу с мхом" },
        new() { name = "Есть хочу" },
        new() { name = "Вовка топ" },
        new() { name = "Wash your foot" },
        new() { name = "Don't eat the elbow" },
        new() { name = "Skillet" },
        new() { name = "Great Slime" },
    };


}
