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


namespace GearSetter
{

    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "GearSetter";

        private const string commandName = "/gs";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }

        private DataManager DataManager { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            DataManager dataManager
            )
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.DataManager = dataManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            /*
            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "schleppin"
            });
            */

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
                if (ImGui.BeginTabItem("Job ID Stuff"))
                {

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
                            //iLevelCSV.GetRow(gearset->Head.ItemID);
                            //var shit = itemCSV.GetRow(gearset->Head.ItemID);
                            //var fuck = iLevelCSV.GetRow(shit)
                            ImGui.Text($"{gearset->Head.ItemID} Head: {itemCSV.GetRow(gearset->Head.ItemID).Name}. Lvl: {itemCSV.GetRow(gearset->Head.ItemID).LevelEquip}");
                            ImGui.Text($"{gearset->Body.ItemID} Body: {itemCSV.GetRow(gearset->Body.ItemID).Name}. Lvl: {itemCSV.GetRow(gearset->Body.ItemID).LevelEquip}");
                            //ImGui.Text($"{gearset->MainHand.ItemID} Main Hand: {itemCSV.GetRow(gearset->MainHand.ItemID).Name}. Lvl: {itemCSV.GetRow(gearset->MainHand.ItemID).LevelEquip}");
                            ImGui.Text($"{gearset->MainHand.ItemID} Main Hand: ");


                        }

                    }
                    
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
