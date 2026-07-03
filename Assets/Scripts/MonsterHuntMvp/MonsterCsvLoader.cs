using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class MonsterCsvLoader
{
    private const string DefaultCsv = "monster_id,name,zone,max_hp,contact_damage,move_speed,chase_range,headbutt_damage_multiplier,stomp_damage_multiplier,exp_reward,gold_reward,spawn_count,is_boss\n" +
        "easy_slime,Green Slime,easy,20,1,1.2,4.0,1.0,1.5,15,5,8,false\n" +
        "easy_mushroom,Forest Mushroom,easy,28,1,1.0,3.5,1.0,1.5,20,7,5,false\n" +
        "normal_boar,Charging Boar,normal,55,2,1.8,6.0,0.9,1.3,45,15,6,false\n" +
        "normal_wolf,Gray Wolf,normal,70,2,2.2,7.0,0.8,1.2,55,18,4,false\n" +
        "hard_golem,Rock Golem,hard,130,3,1.1,6.0,0.7,1.0,95,35,4,false\n" +
        "hard_imp,Red Imp,hard,95,4,2.4,8.0,0.8,1.1,110,40,5,false\n" +
        "boss_ogre,Boss Ogre,boss,650,6,1.5,12.0,0.6,0.9,350,150,1,true";

    public static List<MonsterData> Load()
    {
        string csv = LoadCsvText();
        var result = new List<MonsterData>();
        string[] lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            MonsterData data;
            if (TryParseLine(lines[i], out data))
            {
                result.Add(data);
            }
        }

        return result;
    }

    private static string LoadCsvText()
    {
        TextAsset asset = Resources.Load<TextAsset>("MonsterHunt/monster_data");
        if (asset != null)
        {
            return asset.text;
        }

        string editorPath = Path.Combine(Application.dataPath, "Data/Monsters/monster_data.csv");
        if (File.Exists(editorPath))
        {
            return File.ReadAllText(editorPath);
        }

        Debug.LogWarning("monster_data.csv was not found. Using embedded default monster data.");
        return DefaultCsv;
    }

    private static bool TryParseLine(string line, out MonsterData data)
    {
        data = null;
        string[] c = line.Split(',');
        if (c.Length < 13)
        {
            Debug.LogWarning("Invalid monster csv row: " + line);
            return false;
        }

        HuntZone zone;
        if (!TryParseZone(c[2], out zone))
        {
            Debug.LogWarning("Invalid monster zone: " + c[2]);
            return false;
        }

        try
        {
            data = new MonsterData
            {
                monsterId = c[0],
                name = c[1],
                zone = zone,
                maxHp = int.Parse(c[3], CultureInfo.InvariantCulture),
                contactDamage = int.Parse(c[4], CultureInfo.InvariantCulture),
                moveSpeed = float.Parse(c[5], CultureInfo.InvariantCulture),
                chaseRange = float.Parse(c[6], CultureInfo.InvariantCulture),
                headbuttDamageMultiplier = float.Parse(c[7], CultureInfo.InvariantCulture),
                stompDamageMultiplier = float.Parse(c[8], CultureInfo.InvariantCulture),
                expReward = int.Parse(c[9], CultureInfo.InvariantCulture),
                goldReward = int.Parse(c[10], CultureInfo.InvariantCulture),
                spawnCount = int.Parse(c[11], CultureInfo.InvariantCulture),
                isBoss = bool.Parse(c[12])
            };
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to parse monster csv row: " + line + " / " + ex.Message);
            return false;
        }

        return true;
    }

    private static bool TryParseZone(string value, out HuntZone zone)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "easy":
                zone = HuntZone.Easy;
                return true;
            case "normal":
                zone = HuntZone.Normal;
                return true;
            case "hard":
                zone = HuntZone.Hard;
                return true;
            case "boss":
                zone = HuntZone.Boss;
                return true;
            default:
                zone = HuntZone.Easy;
                return false;
        }
    }
}
