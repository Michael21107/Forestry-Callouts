﻿using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using System.Drawing;
using System;

namespace ForestryCallouts2.Callouts.WaterCallouts
{
    
    [CalloutInfo("BoatSmugglingDrugs", CalloutProbability.Low)]
    
    internal class SmugglerDrugs : Callout
    {
        public override bool OnBeforeCalloutDisplayed()
        {
            
            
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            
            
            return base.OnCalloutAccepted();
        }
    }
    
}