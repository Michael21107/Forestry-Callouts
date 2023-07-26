﻿#region Refrences
//System
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ForestryCallouts2.Backbone.Functions;
//Rage
using Rage;
using Rage.Native;
//ForestryCallouts2
using ForestryCallouts2.Backbone.IniConfiguration;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.Callouts;

#endregion

namespace ForestryCallouts2.Backbone;

public class License
{
    internal string Type { get; private set; }
    private int Chance { get; set; }
    internal string HolderName { get; private set; }
    internal string DateOfBirth { get; private set; }
    internal string HolderGender { get; private set; }

    internal DateTime ExpDate { get; private set; }

    private static Random _rnd = new();

    private static Dictionary<string, License> _fishingDict = new();

    internal static License ChooseTypeOfLicense()
    {
        var licenseList = new List<License>() { };
        licenseList.Add(new License() { Type = "ResidentialFishingLicense", Chance = IniSettings.ResidentLicense });
        licenseList.Add(new License() { Type = "NonResidentialFishingLicense", Chance = IniSettings.NonResidentLicense });
        licenseList.Add(new License() { Type = "OneDayFishingLicense", Chance = IniSettings.OneDayLicense });
        licenseList.Add(new License() { Type = "TwoDayFishingLicense", Chance = IniSettings.TwoDayLicense });
        licenseList.Add(new License() { Type = "null", Chance = IniSettings.NoLicense });
        return SelectLicense(licenseList);
    }

    private static License SelectLicense(List<License> licenses)
    {
        // Calculate the sum.
        var poolSize = licenses.Sum(t => t.Chance);

        // Get a random integer from 0 to PoolSize.
        var randomNumber = _rnd.Next(0, poolSize) + 1;

        // Detect the item, which corresponds to current random number.
        var accumulatedProbability = 0;
        foreach (var t in licenses)
        {
            accumulatedProbability += t.Chance;
            if (randomNumber <= accumulatedProbability)
            {
                Logger.DebugLog("Select License", "Selected " + t.Type);
                return t;
            }
        }
        return null;
    }

    internal static License CreateLicence(in Persona persona, in License license)
    {
        if (_fishingDict.ContainsKey(persona.FullName))
        {
            // Continue to displaying license, ped already has a license in dictionary
            return license;
        }
        else
        {
            //Create a new license for the ped
            license.HolderName = persona.FullName;
            license.DateOfBirth = persona.Birthday.ToShortDateString();
            license.HolderGender = persona.Gender.ToString();
            license.ExpDate = GetExpirationDate(license);

            //Add ped as key and license as val to dict
            _fishingDict.Add(license.HolderName, license);
            return license;
        }
    }
    
    private static DateTime GetExpirationDate(in License license)
    {
        var rawLicenseStatus = GetLicenseStatus();
        var type = license.Type;
        var sysDate = DateTime.Today;
        switch (type)
        {
            case "ResidentialFishingLicense" or "NonResidentialFishingLicense" when rawLicenseStatus.Status == "Expired":
                var minDate = new DateTime(sysDate.Year - 1, sysDate.Month, sysDate.Day);
                var maxDate = new DateTime(sysDate.Year, sysDate.Month, sysDate.Day - 1);
                return GetRandomDateBetween(minDate, maxDate);
            case "ResidentialFishingLicense" or "NonResidentialFishingLicense":
                minDate = new DateTime(sysDate.Year, sysDate.Month, sysDate.Day -1);
                maxDate = new DateTime(sysDate.Year + 1, sysDate.Month, sysDate.Day);
                return GetRandomDateBetween(minDate, maxDate);
            case "OneDayFishingLicense" when rawLicenseStatus.Status == "Expired":
                minDate = new DateTime(sysDate.Year, sysDate.Month, sysDate.Day -7);
                maxDate = new DateTime(sysDate.Year, sysDate.Month, sysDate.Day -1);
                return GetRandomDateBetween(minDate, maxDate);
            case "OneDayFishingLicense":
                return sysDate;
            case "TwoDayFishingLicense" when rawLicenseStatus.Status == "Expired":
                minDate = new DateTime(sysDate.Year, sysDate.Month, sysDate.Day -7);
                maxDate = new DateTime(sysDate.Year, sysDate.Month, sysDate.Day -1);
                return GetRandomDateBetween(minDate, maxDate);
            case "TwoDayFishingLicense":
                minDate = new DateTime(sysDate.Year, sysDate.Month, sysDate.Day);
                maxDate = new DateTime(sysDate.Year + 1, sysDate.Month, sysDate.Day +2);
                return GetRandomDateBetween(minDate, maxDate);
        }
        return sysDate;
    }

    private class LicenseStatus
    {
        internal string Status;
        internal int Chance;
    }
    private static LicenseStatus GetLicenseStatus()
    {
        List<LicenseStatus> statuses = new List<LicenseStatus>();
        statuses.Add(new LicenseStatus() {Status = "Expired", Chance = IniSettings.Expired});
        statuses.Add(new LicenseStatus() {Status = "Valid", Chance = IniSettings.Valid});
        // Calculate the sum.
        var poolSize = statuses.Sum(t => t.Chance);

        // Get a random integer from 0 to PoolSize.
        var randomNumber = _rnd.Next(0, poolSize) + 1;

        // Detect the item, which corresponds to current random number.
        var accumulatedProbability = 0;
        foreach (var t in statuses)
        {
            accumulatedProbability += t.Chance;
            if (randomNumber <= accumulatedProbability)
            {
                Logger.DebugLog("Select License Status", "Selected " + t.Status);
                return t;
            }
        }
        return null;
    }

    private static DateTime GetRandomDateBetween(DateTime startDate, DateTime endDate)
    {
        // Calculate the total number of days between the two dates
        var totalDays = (int)(endDate - startDate).TotalDays;

        // Generate a random number of days between 0 and the total difference in days
        var randomDays = _rnd.Next(totalDays + 1);

        // Add the random number of days to the start date to get an intermediate date
        var intermediateDate = startDate.AddDays(randomDays);

        // Generate random year, month, and day components
        var randomYear = _rnd.Next(intermediateDate.Year, endDate.Year + 1);
        var randomMonth = randomYear == intermediateDate.Year ? _rnd.Next(intermediateDate.Month, 13) : _rnd.Next(1, 13);
        var randomDay = randomYear == intermediateDate.Year && randomMonth == intermediateDate.Month ? _rnd.Next(intermediateDate.Day, DateTime.DaysInMonth(randomYear, randomMonth) + 1) : _rnd.Next(1, DateTime.DaysInMonth(randomYear, randomMonth) + 1);

        // Create the final random date
        return new DateTime(randomYear, randomMonth, randomDay);
    }

    internal static void DisplayLicenceInfo(License license)
    {
        Game.DisplayNotification("commonmenu", "mp_specitem_coke", GetNiceTypeString(license),
            "~h~" + license.HolderName.ToUpper() + "",
            "~y~DOB: ~w~" + license.DateOfBirth + " ~b~SEX: ~o~" + license.HolderGender + " ~g~EXPIRATION DATE:~w~ " +
            license.ExpDate + "");
    }
    
    private static string GetNiceTypeString(License license)
    {
        switch (license.Type)
        {
            case "ResidentialFishingLicense":
                return "RESIDENT FISHING LICENCE";
            case "NonResidentialFishingLicense":
                return "TRAVELER FISHING LICENCE";
            case "OneDayFishingLicense":
                return "ONE DAY FISHING LICENCE";
            case "TwoDayFishingLicense":
                return "TWO DAY FISHING LICENSE";
        }
        return null;
    }
}
