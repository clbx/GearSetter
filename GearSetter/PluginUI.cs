using ImGuiNET;
using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Game.Internal;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.IoC;
using System.Runtime.InteropServices;

namespace GearSetter
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    unsafe class PluginUI : IDisposable
    {

        [PluginService] public static SigScanner SigScanner { get; private set; } = null!;


        internal IntPtr inventoryManager;
        internal delegate InventoryContainer* GetInventoryContainer(IntPtr inventoryManager, int inventoryId);
        internal delegate InventoryItem* GetContainerSlot(InventoryContainer* inventoryContainer, int slotId);
        internal GetInventoryContainer getInventoryContainer;
        internal GetContainerSlot getContainerSlot;

     


        private Configuration configuration;

        private ImGuiScene.TextureWrap goatImage;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration, ImGuiScene.TextureWrap goatImage)
        {
            this.configuration = configuration;
            this.goatImage = goatImage;
        }

        public void Dispose()
        {
            this.goatImage.Dispose();
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            //DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }


            /*
            this.InventoryManager = sig.GetStaticAddressFromSig("BA ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B F8 48 85 C0");
            this.GetInventoryContainer = sig.ScanText("E8 ?? ?? ?? ?? 8B 55 BB"); 
             */

            SigScanner sig = new SigScanner();


            getInventoryContainer = Marshal.GetDelegateForFunctionPointer<GetInventoryContainer>(sig.ScanText("E8 ?? ?? ?? ?? 8B 55 BB"));
   
            getContainerSlot = Marshal.GetDelegateForFunctionPointer<GetContainerSlot>(sig.ScanText("E8 ?? ?? ?? ?? 8B 5B 0C"));

            var container = getInventoryContainer(sig.GetStaticAddressFromSig("BA ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B F8 48 85 C0"), 2008);

            var slot = getContainerSlot(container, 0);

            var itemid = *(uint*)(slot + 0x08);



            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Inventory", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text($"Type: { getContainerSlot.GetType()}");
                ImGui.Text($"Item ID: {itemid}");
            }
            ImGui.End();
        }

  
  


        /**
        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
            if (ImGui.Begin("A Wonderful Configuration Window", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var configValue = this.configuration.SomePropertyToBeSavedAndWithADefault;
                if (ImGui.Checkbox("Random Config Bool", ref configValue))
                {
                    this.configuration.SomePropertyToBeSavedAndWithADefault = configValue;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    this.configuration.Save();
                }
            }
            ImGui.End();
        }
        */
    }
}
