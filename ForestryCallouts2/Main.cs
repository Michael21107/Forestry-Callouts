﻿#region Refrences
//Rage
using System;
using System.Windows.Forms;
using Rage;
//Lspdfr
using LSPD_First_Response.Mod.API;
//Rage Native UI
using RAGENativeUI;
//Forestry Callouts 2
using ForestryCallouts2.Backbone;
using ForestryCallouts2.Backbone.IniConfiguration;
using ForestryCallouts2.Backbone.Menu;
using ForestryCallouts2.Backbone.Functions;
using MainMenu = ForestryCallouts2.Backbone.Menu.MainMenu;

#endregion

namespace ForestryCallouts2
{
    internal class Main : Plugin
    {
        internal static Random Rnd = new Random();
        internal static MenuPool Pool = new();
        private static GameFiber _mainFiber;
        internal static string CallsignAudioString;
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("Plugin ForestryCallouts2 " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " has been initialized");
            Game.LogTrivial("Go on duty to fully load ForestryCallouts2.");
        }
        
        public override void Finally()
        {
            StopPedFiber.Fiber.Abort();
            GrabPedFiber.Fiber.Abort();
            if (AnimalControl.AnimalControlActive) AnimalControl.DestroyAnimalControl();
            MainMenu.CleanUp();
            _mainFiber.Abort();
            Game.LogTrivial("ForestryCallouts2 has been cleaned up.");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            //When player goes on duty FC fully loads
            if (OnDuty)
            {
                Logger.StartLoadingPhase();
                Game.DisplayNotification("commonmenu", "shop_franklin_icon_a", "~g~Forestry Callouts 2", "~g~Plugin Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " ", "Plugin Loaded! Enjoy!");
            }
        }

        //Registers all callouts
        internal static void RegisterCallouts()
        {
            if (!IniSettings.WaterCallouts)
            {
                if (IniSettings.AnimalAttack) Functions.RegisterCallout(typeof(Callouts.LandCallouts.AnimalAttack));
                if (IniSettings.AnimalAttack) Functions.RegisterCallout(typeof(Callouts.LandCallouts.AnimalAttack2));
                if (IniSettings.AnimalOnRoadway) Functions.RegisterCallout(typeof(Callouts.LandCallouts.AnimalOnRoadway));
                if (IniSettings.AtvPursuit) Functions.RegisterCallout(typeof(Callouts.LandCallouts.AtvPursuit));
                if (IniSettings.DangerousPerson) Functions.RegisterCallout(typeof(Callouts.LandCallouts.DangerousPerson));
                if (IniSettings.DeadAnimalOnRoadway) Functions.RegisterCallout(typeof(Callouts.LandCallouts.DeadAnimalOnRoadway));
                if (IniSettings.DirtBikePursuit) Functions.RegisterCallout(typeof(Callouts.LandCallouts.DirtBikePursuit));
                if (IniSettings.HighSpeedPursuit) Functions.RegisterCallout(typeof(Callouts.LandCallouts.HighSpeedPursuit)); 
                if (IniSettings.IntoxPerson) Functions.RegisterCallout(typeof(Callouts.LandCallouts.IntoxicatedPerson));
                if (IniSettings.LoggerTruckPursuit) Functions.RegisterCallout(typeof(Callouts.LandCallouts.LoggerTruckPursuit));
                if (IniSettings.RegularPursuit) Functions.RegisterCallout(typeof(Callouts.LandCallouts.RegularPursuit));
            }
            else
            {
                if (IniSettings.DeadBodyWater) Functions.RegisterCallout(typeof(Callouts.WaterCallouts.DeadBodyWater));
                if (IniSettings.BoatPursuit) Functions.RegisterCallout(typeof(Callouts.WaterCallouts.BoatPursuit));
            }
        }
        
        //GameFiber that runs constantly for interaction menu and binoculars
        internal static void RunLoop()
        {
            _mainFiber = GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();
                    

                    //Menu
                    Pool.ProcessMenus();
                    if (CFunctions.IsKeyAndModifierDown(IniSettings.InteractionMenuKey, IniSettings.InteractionMenuKeyModifier) && !Binoculars.IsRendering)
                    {
                        if (MainMenu.InteractionMenu.Visible) MainMenu.InteractionMenu.Visible = false;
                        else MainMenu.InteractionMenu.Visible = true;
                    }

                    //Binoculars Hotkey
                    if (CFunctions.IsKeyAndModifierDown(IniSettings.BinocularsKey, IniSettings.BinocularsKeyModifier) && IniSettings.BinocularsEnabled && !Binoculars.IsRendering && Game.LocalPlayer.Character.IsOnFoot && Binoculars.BinoKeyEnabled)
                    {
                        Binoculars.Enable();
                    }
                }
            });
        }
    }
}