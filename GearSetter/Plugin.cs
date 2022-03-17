using System.IO;
using System.Reflection;
using System.Numerics;
using ImGuiNET;

using Dalamud.Interface;

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

using System;
using System.Runtime.InteropServices;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Excel;

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace GearSetter
{

    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "GearSetter";

        private const string commandName = "/gs";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private GameGui GameGui { get; init; }
        private DataManager DataManager { get; init; }

        public GameGui gui; 

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            DataManager dataManager,
            GameGui gameGui
            )
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.DataManager = dataManager;
            this.GameGui = gameGui;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);


            gui = this.GameGui;


            this.PluginInterface.UiBuilder.Draw += DrawUI;

        }



        public void Dispose()
        {
            this.CommandManager.RemoveHandler(commandName);
        }

        /**
        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.PluginUi.Visible = true;

        }**/

        private unsafe AtkResNode* FindNodeByID(int id, AtkUnitBase* unitBase)
        {
            for (int i = 0; i < unitBase->UldManager.NodeListCount; i++)
            {
                //Really Hope this is always 14
                if (unitBase->UldManager.NodeList[i]->NodeID == id)
                {
                    return unitBase->UldManager.NodeList[i];
                }
            }
            return null;
        }
        private unsafe void DrawUI()
        {

            var gearsets = RaptureGearsetModule.Instance();
            var inventoryManager = InventoryManager.Instance();
            var inventory = inventoryManager->GetInventoryContainer(InventoryType.Inventory1);
            var itemCSV = DataManager.GetExcelSheet<Item>()!;
            var jobCSV = DataManager.GetExcelSheet<ClassJobCategory>()!;
            var iLevelCSV = DataManager.GetExcelSheet<ItemLevel>()!;
           
            

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            ImGui.Begin("GearSetter");

            if (ImGui.BeginTabBar("Main"))
            {
                if (ImGui.BeginTabItem("Job ID asdfStuff"))
                {

                    /**
                    //https://ffxiv.pf-n.co/xivapi?url=%2FClassJob

                    var gearsetModule = RaptureGearsetModule.Instance();

                    var gearset = gearsetModule->Gearset[0];
                    var i = 0;

                    while(gearset->ClassJob != 0)
                    {
                        gearset = gearsetModule->Gearset[i];
                        var gearsetName = Marshal.PtrToStringAnsi((IntPtr)gearset->Name);
                        var gearsetID = gearset->ID;
                        i++;

                        if(gearset->ClassJob == 0)
                        {
                            break;
                        }
                        
                        if(ImGui.CollapsingHeader($"{gearset->ID}: { gearsetName}  {gearset->ClassJob}"))
                        {
                            iLevelCSV.GetRow(gearset->Head.ItemID);
                            var shit = itemCSV.GetRow(gearset->Head.ItemID);
                            var poop = shit.LevelItem;
                            ImGui.Text($"{gearset->Head.ItemID} Head: {itemCSV.GetRow(gearset->Head.ItemID).Name}. Lvl: {itemCSV.GetRow(gearset->Head.ItemID).LevelEquip} iLvl: {poop}");
                            ImGui.Text($"{gearset->Body.ItemID} Body: {itemCSV.GetRow(gearset->Body.ItemID).Name}. Lvl: {itemCSV.GetRow(gearset->Body.ItemID).LevelEquip}");
                            //ImGui.Text($"{gearset->MainHand.ItemID} Main Hand: {itemCSV.GetRow(gearset->MainHand.ItemID).Name}. Lvl: {itemCSV.GetRow(gearset->MainHand.ItemID).LevelEquip}");
                            ImGui.Text($"{gearset->MainHand.ItemID} Main Hand: ");


                        }

                    }*/

                    //Iterate through classes
                    var characterWindowPtr = gui.GetAddonByName("Character", 1);
                    var characterWindow = (AtkUnitBase*)characterWindowPtr;
                    ImGui.Text($"Character Addr:{characterWindowPtr.ToString("X")}");

                   

                    var boundingBox = FindNodeByID(11, characterWindow);
                    //var gearSetLabel = FindNodeByID(15,characterWindow);
                    var gearSetSelectedLabel = FindNodeByID(16, characterWindow);
                    var gearSetLabel = (AtkTextNode*)FindNodeByID(15, characterWindow);
                    var gearSetLabelEncapulate = FindNodeByID(16, characterWindow);
                    gearSetLabelEncapulate->X = 1000;




                    ImGui.EndTabItem();

                }
                if (ImGui.BeginTabItem("Inventory"))
                {
                    

                    for (int i = 0; i < inventory->Size; i++)
                    {

                        //Class Job Thing

                        ImGui.Text($"Item ID: {inventory->Items[i].ItemID}");

                        //The row has a lot of useful information;
                        var row = itemCSV.GetRow((uint)inventory->Items[i].ItemID);

                        //This has the job code for the item https://ffxiv.pf-n.co/xivapi?url=%2FClassJobCategory for more info. 
                        var jobRowID = row.ClassJobCategory.Row;
                        var job = jobCSV.GetRow(jobRowID);

                        ImGui.Text($"{row.Name}");
                        ImGui.Text($"MCH Equip? {job.MCH}");


                        ImGui.Text($"");
                        ImGui.EndTabItem();
                    }
                    
                }
                if (ImGui.BeginTabItem("Settings"))
                {

                    ImGui.EndTabItem();
                }

            }
            ImGui.End();

            ImGui.ShowDemoWindow();
        }

    }
}
