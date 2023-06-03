﻿#region Refrences
//System
using System.Drawing;
//Rage
using Rage;
#endregion

namespace ForestryCallouts2.Backbone.Functions
{
    internal class AnimalControl
    {
        private static Ped _animal;
        private static Ped[] _allPeds;
        private static Vehicle _acVehicle;
        private static Ped _acPed;
        private static Blip _acBlip;
        
        internal static void CallAnimalControl()
        {
            Logger.DebugLog("ANIMAL CONTROL", "Animal Control has been called");
            Logger.DebugLog("ANIMAL CONTROL", "Finding closest dead animal");
            _allPeds = World.GetAllPeds();
            
            foreach (var ped in _allPeds)
            {
                if (Game.LocalPlayer.Character.DistanceTo(ped) <= 10f && !ped.IsHuman && ped.IsDead)
                {
                    Logger.DebugLog("GetClosestPed", ped.Model.Name);
                    _animal = ped;
                }
            }
            
            //return if animal is null
            if (_animal == null)
            {
                Game.DisplayNotification("~g~Could Not Find Dead Animal");
                Logger.DebugLog("ANIMAL CONTROL", "Failed to find dead Animal");
                return;
            }
            
            //yeah cool stuff man
            Game.DisplayNotification("~b~Dispatch:~w~ Animal Control is in route to your location. ");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("OFFICERS_REPORT_03 ASSISTANCE_REQUIRED_02");

            //spawnpoint position
            var startPosition = Game.LocalPlayer.Character.Position.Around(150f, 200f);
            var finalStartPosition = World.GetNextPositionOnStreet(startPosition);
            var animalPos = _animal.Position;
            var aroundAnimalPos = animalPos.Around2D(150f);
            var finalPosition = World.GetNextPositionOnStreet(finalStartPosition);

            //spawn animal control vehicle
            CFunctions.SpawnAnimalControl(out _acVehicle, finalPosition, 0f);
            _acVehicle.IsPersistent = true;

            _acVehicle.PrimaryColor = Color.White;

            //spawn animal control ped
            _acPed = new Ped("s_f_y_ranger_01", finalPosition, 0f);
            _acPed.BlockPermanentEvents = true;
            _acPed.IsPersistent = true;

            //set blip to animal control ped
            _acBlip = _acPed.AttachBlip();
            _acBlip.Color = Color.ForestGreen;

            //warp ac ped into ac vehicle
            _acPed.WarpIntoVehicle(_acVehicle, -1);

            //Get drive to postion for animal control
            var closeToAnimalPos = animalPos.Around2D(5f);
            var closeToFinalPos  = World.GetNextPositionOnStreet(closeToAnimalPos);

            //Animal control drive to position
            _acPed.Tasks.DriveToPosition(closeToFinalPos, 9f, VehicleDrivingFlags.Normal).WaitForCompletion();

            //get ac to leave vehicle when on scene
            _acPed.Tasks.LeaveVehicle(_acVehicle, LeaveVehicleFlags.None).WaitForCompletion();
            Logger.DebugLog("ANIMAL CONTROL", "Animal Control officer leaving vehicle");

            //ac walks to animal
            _acPed.Tasks.FollowNavigationMeshToPosition(_animal.Position, _animal.Heading + 180f, 10f, -1).WaitForCompletion(); 
            Game.DisplaySubtitle("~g~Animal Control:~w~ Thank you, I will take care of the animal from here.");
            
            //deletes the dead animal when animal control is right next to the animal.
            if (_animal.Exists())
            {
                _animal.Delete();
                _animal = null;
            }

            //Tells the animal control to get back into the truck
            _acPed.Tasks.GoStraightToPosition(_acVehicle.Position.Around2D(2f, 4f), 10f, _acVehicle.Heading + 180f, 0f, -1).WaitForCompletion();
            _acPed.Tasks.EnterVehicle(_acVehicle, -1).WaitForCompletion();

            //Tells the animal control to drive off
            _acPed.Tasks.CruiseWithVehicle(10f, VehicleDrivingFlags.Normal);
            
            //Dismisses everything
            destroyAnimalControl();
        }

        internal static void destroyAnimalControl()
        {
            if (_acPed.Exists())
            {
                _acPed.Dismiss();
            }
            if (_acVehicle.Exists())
            {
                _acVehicle.Dismiss();
            }
            if (_acBlip.Exists())
            {
                _acBlip.Delete();
            }
        }
    }
}
