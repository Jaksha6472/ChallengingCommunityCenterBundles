using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using System.Text.RegularExpressions;

namespace CCCB
{
    public class CCCB : Mod
    {
        private ModConfig Config;
        private Dictionary<string, string> CustomBundleData;
        private Dictionary<string, Dictionary<string, string>> BundlePacks = new Dictionary<string, Dictionary<string, string>>();

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            string BundleVariant = this.Config.BundleVariant;

            // Check the requirements for certain content packs
            if (this.Config.BundleVariant == "PPJA")
            {
                if (!this.Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets"))
                {
                    this.Monitor.Log("You can't use the PPJA bundle variant without JsonAssets installed", LogLevel.Error);
                    return;
                }
                if (!this.Helper.ModRegistry.IsLoaded("ppja.artisanvalleymachinegoods"))
                {
                    this.Monitor.Log("You can't use the PPJA bundle variant without PPJA Artisan Valley installed", LogLevel.Error);
                    return;
                }
                if (!this.Helper.ModRegistry.IsLoaded("ppja.fruitsandveggies"))
                {
                    this.Monitor.Log("You can't use the PPJA bundle variant without PPJA Fruits and Veggies installed", LogLevel.Error);
                    return;
                }
                if (!this.Helper.ModRegistry.IsLoaded("mizu.flowers"))
                {
                    this.Monitor.Log("You can't use the PPJA bundle variant without PPJA Mizu's Flowers installed", LogLevel.Error);
                    return;
                }
                if (!this.Helper.ModRegistry.IsLoaded("paradigmnomad.morefood"))
                {
                    this.Monitor.Log("You can't use the PPJA bundle variant without PPJA More Recipes installed", LogLevel.Error);
                    return;
                }
                if (!this.Helper.ModRegistry.IsLoaded("ppja.moretrees"))
                {
                    this.Monitor.Log("You can't use the PPJA bundle variant without PPJA More Trees installed", LogLevel.Error);
                    return;
                }
                if (!this.Helper.ModRegistry.IsLoaded("ppja.evenmorerecipes"))
                {
                    this.Monitor.Log("You can't use the PPJA bundle variant without PPJA Even More Recipes installed", LogLevel.Error);
                    return;
                }
            }

            // Load all the Content Packs
            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
            {
                if (this.BundlePacks.Count == 0)
                {
                    this.BundlePacks = contentPack.ReadJsonFile<Dictionary<string, Dictionary<string, string>>>("content.json");
                }
                else
                {
                    Dictionary<string, Dictionary<string, string>> BundlePacksTemp = contentPack.ReadJsonFile<Dictionary<string, Dictionary<string, string>>>("content.json");
                    foreach (KeyValuePair<string, Dictionary<string, string>> entry in BundlePacksTemp)
                    {
                        this.BundlePacks.Add(entry.Key, entry.Value);
                    }
                }

            }

            // Check which Content Packs are loaded
            foreach (KeyValuePair<string, Dictionary<string, string>> entry in this.BundlePacks)
            {
                this.Monitor.Log($"Loaded Bundle: " + entry.Key, LogLevel.Info);
            }

            if (BundlePacks.ContainsKey(this.Config.BundleVariant))
            {
                this.CustomBundleData = BundlePacks[this.Config.BundleVariant];
                this.Monitor.Log($"Game will be using " + this.Config.BundleVariant + " Bundles", LogLevel.Info);
            }
            else
            {
                this.Monitor.Log($"Your Config Settings are invalid", LogLevel.Error);
                return;
            }

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        private void OnSaveLoaded(object sender, EventArgs e)
        {
            if (Context.IsWorldReady)
            {
                var api = this.Helper.ModRegistry.GetApi<IJsonAssetsAPI>("spacechase0.JsonAssets");

                Dictionary<string, string> BundleData = Game1.netWorldState.Value.BundleData;
                for (int index = 0; index < BundleData.Count; index++)
                {
                    string[] BundleName = BundleData.ElementAt(index).Value.Split('/'); //The Name of the Game Bundles
                    string Key = BundleData.ElementAt(index).Key;

                    if (this.CustomBundleData.ContainsKey(BundleName[0]))
                    {
                        string pattern = @"(?<=ObjectId:)[A-Za-z ]+";

                        foreach (Match result in Regex.Matches(this.CustomBundleData[BundleName[0]], pattern))
                        {
                            int objectId = api.GetObjectId(result.ToString());
                            if (api != null)
                            {
                                this.CustomBundleData[BundleName[0]] = this.CustomBundleData[BundleName[0]].Replace("{{spacechase0.jsonAssets/ObjectId:" + result.ToString() + "}}", objectId.ToString());
                            }
                        }

                        this.Monitor.Log($"Set Bundle " + BundleData.ElementAt(index).Key + " to " + this.CustomBundleData[BundleName[0]], LogLevel.Info);
                        Game1.netWorldState.Value.BundleData[Key] = this.CustomBundleData[BundleName[0]];
                    }
                }
            }
        }
    }

    public interface IJsonAssetsAPI
    {
        int GetObjectId(string name);
    }
}
