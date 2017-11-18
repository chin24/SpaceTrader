﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeControl;
/// <summary>
/// Holds all the game data to be saved in a save file
/// </summary>
public class GameDataModel : Model {
    public string galaxyName;
    public Dated date = new Dated();
    public int id = 0;
    public int playerCreatureId;

    public static double G = 6.67e-11;
    public static double galaxyDistanceMultiplication = 9.461e+11 ; // Lightyear in km = 9.461e12.
    public static string galaxyDistanceUnit = "ly";
    public static double solarDistanceMultiplication = Units.M * 100;
    public static string solarDistanceUnit = "Mm";
    public static double sunMassMultiplication = 1.969e+30;
    public static string sunMassUnit = @"M\u+0298";

    //Camera
    public Vector2d cameraGalaxyPosition = Vector2d.zero;
    public Vector2d cameraSolarPosition = Vector2d.zero;
    public double cameraGalaxyOrtho = 100 * Units.ly / galaxyDistanceMultiplication;
    public double cameraSolarOrtho = 10000000;
    public double cameraGalCameraScaleMod = 1;
    public double cameraSolCameraScaleMod = 1;
    public double distanceDivision = 9.461e+15; // Lightyear.

    public static float galaxyCameraScaleMax = 500 * (float)(Units.ly / GameDataModel.galaxyDistanceMultiplication);

    public ModelRef<ShipsModel> ships = new ModelRef<ShipsModel>(new ShipsModel());
    public ModelRef<CreatureModel> creatures = new ModelRef<CreatureModel>(new CreatureModel());
    public ModelRef<ItemsModel> items = new ModelRef<ItemsModel>(new ItemsModel());
    public ModelRef<RawResourcesModel> rawResources = new ModelRef<RawResourcesModel>(new RawResourcesModel());

    public ModelRefs<SolarModel> stars = new ModelRefs<SolarModel>();
   
    public ModelRefs<GovernmentModel> governments = new ModelRefs<GovernmentModel>();
    public ModelRefs<CompanyModel> companies = new ModelRefs<CompanyModel>();
    
    public Research[] research = new Research[] {new Research("Hyperdrive","Allows ships to travel at faster than light speeds", 1000, new string[] { }),
        new Research("Jumpdrive","Allows ships to jump to other star systems", 10000, new string[] {"Hyperdrive"}),
        new Research("Quarium Material","More durable ship building material", 2500, new string[] { }),
        new Research("Micro-Quarium Material","Allows ships to travel at faster than light speeds", 5000, new string[] {"Quarium Material" })
    };

    //----------------------------Solar Display Settings-------------------//

    public SolarBody getSolarBody(List<int> solarIndex)
    {
        if (solarIndex.Count == 1)
        {
            return stars[solarIndex[0]].solar;
        }
        if (solarIndex.Count == 2)
        {
            return stars[solarIndex[0]].solar.satelites[solarIndex[1]];
        }
        if (solarIndex.Count == 3)
        {
            return stars[solarIndex[0]].solar.satelites[solarIndex[1]].satelites[solarIndex[2]];
        }
        throw new System.Exception("Solar Index invalid");
    }
}
