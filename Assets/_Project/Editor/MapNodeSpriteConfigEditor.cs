using System.Linq;
using UnityEngine;
using UnityEditor;

public static class MapNodeSpriteConfigEditor
{
    const string MapPath = "Assets/_Project/Materials/UI/Map";
    const string ConfigPath = "Assets/_Project/Materials/UI/Map/MapNodeSpriteConfig.asset";

    [MenuItem("StitchedSouls/Create Map Node Sprite Config")]
    static void CreateMapNodeSpriteConfig()
    {
        var config = AssetDatabase.LoadAssetAtPath<MapNodeSpriteConfig>(ConfigPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<MapNodeSpriteConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }

        config.normal = new MapNodeSpriteConfig.NodeSpriteSet
        {
            active = LoadSprite("UI_Node_Normal_Active"),
            inactive01 = LoadSprite("UI_Node_Normal_Inactive_01"),
            inactive02 = LoadSprite("UI_Node_Normal_Inactive_02")
        };
        config.elite = new MapNodeSpriteConfig.NodeSpriteSet
        {
            active = LoadSprite("UI_Node_Elite_Active"),
            inactive01 = LoadSprite("UI_Node_Elite_Inactive_01"),
            inactive02 = LoadSprite("UI_Node_Elite_Inactive_02")
        };
        config.shop = new MapNodeSpriteConfig.NodeSpriteSet
        {
            active = LoadSprite("UI_Node_Shop_Active"),
            inactive01 = LoadSprite("UI_Node_Shop_Inactive_01"),
            inactive02 = LoadSprite("UI_Node_Shop_Inactive_02")
        };

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Selection.activeObject = config;
        Debug.Log($"MapNodeSpriteConfig created/updated at {ConfigPath}");
    }

    static Sprite LoadSprite(string name)
    {
        var path = $"{MapPath}/{name}.png";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        return sprite;
    }
}
