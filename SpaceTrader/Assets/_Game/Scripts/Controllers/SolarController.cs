﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CodeControl;
using System;
using Vectrosity;

public class SolarController : Controller<SolarModel> {

    public GameObject sunObj;
    public GameObject planetObj;
    public GameObject moonObj;
    internal GameObject sun;
    internal List<GameObject> planets;
    internal List<GameObject> moons = new List<GameObject>();
    internal List<SolarBody> moonModels = new List<SolarBody>();
    internal GameManager game;
    internal GalaxyManager galaxy;
    private SpriteRenderer sprite;
    private CircleCollider2D circleCollider;
    public float viewPopulationSize = 3f;
    public float viewCompanySize = 1;
    private float solarSpriteScale = .02f;
    private float moonViewOrthoSize = .001f;
    private float lineSize = 2;
    //private VectorObject3D line;
    private VectorLine solarLine;
    public Texture lineTexture;
    internal double supplyDemandProgress = Dated.Hour- 30;
    public double supplyDemandUpdateTime = Dated.Hour;

    internal double populationUpdateProgress = 0;
    internal double populationUpdateTime = Dated.Day;

    private Vector3 lastCamPosition = Vector3.zero;
    protected override void OnInitialize()
    {
        lastCamPosition.z = -10;
        game = GameManager.instance;
        galaxy = GalaxyManager.instance;
        transform.position = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition);
        name = model.name;
        circleCollider = GetComponent<CircleCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = model.solar.color;
        transform.localScale = Vector3.one;
        var points = new List<Vector3>();
        
        solarLine = new VectorLine("model.name Connections", points, (float)(Mathd.Pow((model.solar.bodyRadius), .02f) * game.data.cameraGalCameraScaleMod));
        //line.SetVectorLine(vectorLine, lineTexture, sprite.material);
        sprite.enabled = false;

        //staggers the updating of supplydemand and population updates
        supplyDemandProgress = Dated.Hour - UnityEngine.Random.Range(0, 300);
        populationUpdateProgress = UnityEngine.Random.Range(0, 300);

        if (model.isActive)
        {
            CreateSystem();
        }
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition);
        circleCollider.radius = (float)(Mathd.Pow((model.solar.bodyRadius), .02f) * game.data.cameraGalCameraScaleMod * 5);
        //Population
        populationUpdateProgress += game.data.date.deltaTime;
        if (populationUpdateProgress > populationUpdateTime)
        {

            for (int i = 0; i < model.solar.satelites.Count; i++)
            {
                SolarBody body = model.solar.satelites[i];
                if (body.inhabited)
                {
                    body.population.Update(body, populationUpdateProgress);
                    if (body.sateliteInhabited)
                    {
                        for (int m = 0; m < body.satelites.Count; m++)
                        {
                            SolarBody moon = body.satelites[m];
                            if (moon.inhabited)
                            {
                                moon.population.Update(moon, populationUpdateProgress);
                            }
                        }
                    }
                }

            }
            populationUpdateProgress = 0;
        }
        //Commerce
        supplyDemandProgress += game.data.date.deltaTime;
        if (supplyDemandProgress > supplyDemandUpdateTime)
        {
            foreach (Item item in model.buyList.items)
            {
                int index = model.supplyDemand.FindIndex(x => x.itemId == item.id);

                if (index >= 0)
                {

                }
                else
                {
                    model.supplyDemand.Add(new SupplyDemand(item.name, item.id, item.estimatedValue, 0, item.amount));
                }
            }
            foreach (Item item in model.sellList.items)
            {
                int index = model.supplyDemand.FindIndex(x => x.itemId == item.id);

                if (index >= 0)
                {

                }
                else
                {
                    model.supplyDemand.Add(new SupplyDemand(item.name, item.id, item.estimatedValue, item.amount, 0));
                }
            }
            foreach (SupplyDemand x in model.supplyDemand)
            {
                x.itemDemand = 0;
                x.averageItemBuyPrice = 0;
                model.buyList.items.FindAll(d => d.id == x.itemId).ForEach(i => { x.itemDemand += i.amount; x.averageItemBuyPrice += i.price * i.amount; });
                x.averageItemBuyPrice /= x.itemDemand+1;

                x.itemSupply = 0;
                x.averageItemSellPrice = 0;
                model.sellList.items.FindAll(d => d.id == x.itemId).ForEach(i => { x.itemSupply += i.amount; x.averageItemSellPrice += i.price * i.amount; });
                x.averageItemSellPrice /= x.itemSupply+1;

                if (x.itemSupply == 0 && x.itemDemand == 0) 
                {
                    if (x.marketPrice == 0)
                    model.supplyDemand.Remove(x);
                    break;
                }

                x.marketPrice = (x.itemDemand * x.averageItemBuyPrice + x.itemSupply * x.averageItemSellPrice) / (x.itemDemand + x.itemSupply);

                if (x.marketPrice < 0)
                {
                    x.marketPrice = 0;
                }

            };
            supplyDemandProgress = 0;
        }

        List<Vector3> linePoints = new List<Vector3>();
        List<Color32> lineColors = new List<Color32>();
        List<float> lineWidths = new List<float>();
        if (model.isActive)
        {
            sun.transform.position = transform.position;
            var localScale = (float)(Mathd.Pow((model.solar.bodyRadius), solarSpriteScale) * Mathd.Pow(game.data.cameraGalCameraScaleMod, .9f));
            if (localScale * game.data.cameraGalaxyOrtho / Camera.main.orthographicSize * GameDataModel.galaxyDistanceMultiplication < model.solar.bodyRadius)
            {
                localScale = (float)(model.solar.bodyRadius / GameDataModel.galaxyDistanceMultiplication * Camera.main.orthographicSize / game.data.cameraGalaxyOrtho);
            }
            sun.transform.localScale = Vector3.one * localScale;

            for (int i = 0; i < model.solar.satelites.Count; i++)
            {
                SolarBody body = model.solar.satelites[i];
                //planets[i].transform.localScale = sun.transform.localScale * .5f;
                localScale = (float)(Mathd.Pow((body.bodyRadius), solarSpriteScale) * Mathd.Pow(game.data.cameraGalCameraScaleMod, .9f));
                if (localScale * game.data.cameraGalaxyOrtho / Camera.main.orthographicSize * GameDataModel.galaxyDistanceMultiplication < body.bodyRadius)
                {
                    localScale = (float)(body.bodyRadius / GameDataModel.galaxyDistanceMultiplication * Camera.main.orthographicSize / game.data.cameraGalaxyOrtho);
                }
                planets[i].transform.localScale = Vector3.one * localScale;
                planets[i].transform.position = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + body.lastKnownPosition);

                //Orbit Line Rendering
                LineRenderer line = planets[i].GetComponent<LineRenderer>();

                if (MapTogglePanel.instance.solarOrbits.isOn && !canSeeMoons())
                {
                    line.widthMultiplier = planets[i].transform.localScale.x * .3f;
                    
                    //Creates the line rendering for the orbit of planets

                    Vector3[] positions = new Vector3[36];

                    for (var b = 0; b < 36; b++)
                    {

                        positions[b] = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + body.approximatePositions[b * 10]);
                    }
                    line.positionCount = 36;
                    line.SetPositions(positions);
                }
                else
                {
                    line.widthMultiplier = 0;
                }

                //Population rendering
                if (MapTogglePanel.instance.populations.isOn && (body.inhabited || body.sateliteInhabited))
                {
                    if (canSeeMoons() && body.inhabited)
                    {
                        linePoints.Add(planets[i].transform.position + Vector3.up * planets[i].transform.localScale.x * .5f);
                        linePoints.Add(planets[i].transform.position + Vector3.up * planets[i].transform.localScale.x * .5f + Vector3.up * viewPopulationSize);
                        lineColors.Add(new Color32(100, 100, 255, 200));
                        lineWidths.Add(localScale);
                    }
                    else if (body.inhabited || body.sateliteInhabited)
                    {
                        linePoints.Add(planets[i].transform.position + Vector3.up * planets[i].transform.localScale.x * .5f);
                        linePoints.Add(planets[i].transform.position + Vector3.up * planets[i].transform.localScale.x * .5f + Vector3.up * viewPopulationSize);
                        lineColors.Add(new Color32(100, 100, 255, 200));
                        lineWidths.Add(localScale);
                    }


                }

                //Companies rendering
                if (MapTogglePanel.instance.compines.isOn)
                {
                    linePoints.Add(planets[i].transform.position + Vector3.right * planets[i].transform.localScale.x * .5f);
                    linePoints.Add(planets[i].transform.position + Vector3.right * planets[i].transform.localScale.x * .5f + Vector3.right * Mathf.Pow(body.companies.Count, viewCompanySize) * lineSize);

                    if (!canSeeMoons())
                    {
                        var index = linePoints.Count - 1;
                        body.satelites.ForEach(x => linePoints[index] += Vector3.right * Mathf.Pow(x.companies.Count, viewCompanySize));                        
                    }

                    lineColors.Add(new Color32(255, 50, 255, 150));
                    lineWidths.Add(localScale);
                }
            }
            for (int i = 0; i < moons.Count; i++)
            {
                SolarBody body = moonModels[i];
                moons[i].transform.position = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + model.solar.satelites[body.solarIndex[1]].lastKnownPosition
                        + body.lastKnownPosition);

                if (canSeeMoons() && moons[i].transform.position.sqrMagnitude < 62500)
                {
                    var visible = CheckVisibility(body);
                    moons[i].SetActive(visible);
                    LineRenderer line = moons[i].GetComponent<LineRenderer>();

                    //moons[i].transform.localScale = sun.transform.localScale * .15f;
                    localScale = (float)(Mathd.Pow((body.bodyRadius), solarSpriteScale) * Mathd.Pow(game.data.cameraGalCameraScaleMod, .5f));
                    if (localScale * game.data.cameraGalaxyOrtho / Camera.main.orthographicSize * GameDataModel.galaxyDistanceMultiplication < body.bodyRadius)
                    {
                        localScale = (float)(body.bodyRadius / GameDataModel.galaxyDistanceMultiplication * Camera.main.orthographicSize / game.data.cameraGalaxyOrtho);
                    }
                    moons[i].transform.localScale = Vector3.one * localScale;

                    //Creates the line rendering for the orbit of moons

                    if (MapTogglePanel.instance.solarOrbits.isOn)
                    {
                        line.widthMultiplier = moons[i].transform.localScale.x * .3f;

                        var positions = new Vector3[36];
                        for (var b = 0; b < 36; b++)
                        {
                            positions[b] = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + model.solar.satelites[body.solarIndex[1]].lastKnownPosition
                        + body.approximatePositions[b * 10]);
                        }
                        line.positionCount = 36;
                        line.SetPositions(positions);
                    }
                    else
                    {
                        line.widthMultiplier = 0;
                    }

                    if (moons[i].activeSelf)
                    {
                        //Population rendering
                        if (MapTogglePanel.instance.populations.isOn && body.inhabited)
                        {
                            linePoints.Add(moons[i].transform.position + Vector3.up * moons[i].transform.localScale.x * .5f);
                            linePoints.Add(moons[i].transform.position + Vector3.up * moons[i].transform.localScale.x * .5f + Vector3.up * viewPopulationSize);
                            lineColors.Add(new Color32(100, 100, 255, 200));
                            lineWidths.Add(localScale);
                        }

                        //Companies rendering
                        if (MapTogglePanel.instance.compines.isOn)
                        {
                            linePoints.Add(moons[i].transform.position + Vector3.right * moons[i].transform.localScale.x * .5f);
                            linePoints.Add(moons[i].transform.position + Vector3.right * moons[i].transform.localScale.x * .5f + Vector3.right * Mathf.Pow(body.companies.Count, viewCompanySize) * lineSize);

                            lineColors.Add(new Color32(255, 50, 255, 150));
                            lineWidths.Add(localScale);
                        }
                    }
                    
                }
                else
                {
                    moons[i].SetActive(false);
                }

                

            }

            

            if (game.data.cameraGalaxyOrtho * GameDataModel.galaxyDistanceMultiplication > 1.5 * Units.ly)
            {
                galaxy.GalaxyView();
            }
        }
        else
        {
            if (transform.position.sqrMagnitude < 40000)
            {
                if (MapTogglePanel.instance.galaxyConnections.isOn)
                {
                    foreach (SolarModel solar in model.nearStars)
                    {
                        linePoints.Add(transform.position);
                        linePoints.Add(CameraController.CameraOffsetGalaxyPosition(solar.galaxyPosition));
                        lineWidths.Add((float)circleCollider.radius);
                        if (MapTogglePanel.instance.galaxyTerritory.isOn)
                        {
                            if (model.government.Model != null)
                            {
                                var govModel = model.government.Model;
                                lineColors.Add(new Color32((byte)(govModel.spriteColor.r * 255), (byte)(govModel.spriteColor.g * 255), (byte)(govModel.spriteColor.b * 255), 50));
                            }
                            else
                            {
                                lineColors.Add(new Color32((byte)(50), (byte)(50), (byte)(50), 10));
                            }

                        }
                        else
                        {
                            lineColors.Add(new Color32((byte)(model.solar.color.r * 255), (byte)(model.solar.color.g * 255), (byte)(model.solar.color.b * 255), 10));
                        }
                    }
                    
                }
                else
                {
                    //if (solarLine.lineWidth != 0)
                    //{
                    //    solarLine.SetWidth(0);
                    //    solarLine.Draw3D();
                    //}
                }

                //Population rendering
                if (MapTogglePanel.instance.populations.isOn && model.solar.inhabited)
                {
                    linePoints.Add(transform.position + Vector3.up * circleCollider.radius * .5f);
                    linePoints.Add(transform.position + Vector3.up * circleCollider.radius + Vector3.up * viewPopulationSize);
                    lineColors.Add(new Color32(100, 100, 255, 200));
                    lineWidths.Add(circleCollider.radius * 2f);
                }
            }
            else
            {
                //if (solarLine.lineWidth != 0)
                //{
                //    solarLine.SetWidth(0);
                //    solarLine.Draw3D();
                //}
            }
        }
        //Stats Display
        if (linePoints.Count > 0)
        {
            solarLine.points3 = linePoints;
            solarLine.SetWidths(lineWidths);
            solarLine.SetColors(lineColors);
            solarLine.Draw3D();
        }
        else
        {
            solarLine.points3 = new List<Vector3>();
            solarLine.lineWidth = 0;
            solarLine.Draw();
        }
        

        ToggleSystem();
		
	}

    private bool canSeeMoons()
    {
        return game.data.cameraGalaxyOrtho < moonViewOrthoSize;
    }

    public IEnumerator UpdateSolarObjects()
    {
        while (model.isActive)
        {
            for (int i = 0; i < model.solar.satelites.Count; i++)
            {
                SolarBody body = model.solar.satelites[i];
                Vector2 position = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + body.GamePosition(game.data.date.time));

                Vector3 screenPoint = Camera.main.WorldToViewportPoint(position);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

                if (onScreen)
                {
                    var visible = CheckVisibility(body);

                    planets[i].SetActive(visible);

                    planets[i].transform.position = position;

                    LineRenderer line = planets[i].GetComponent<LineRenderer>();

                    //Creates the line rendering for the orbit of planets

                    Vector3[] positions = new Vector3[361];
                    double time = game.data.date.time;
                    body.SetOrbit(time, model.solar.mass);


                    for (var b = 0; b < 360; b++)
                    {
                        positions[b] = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + body.approximatePositions[b]);
                    }
                    line.positionCount = 360;
                    line.SetPositions(positions);

                    yield return null;
                }
            }

            for (int i = 0; i < moons.Count; i++)
            {
                SolarBody moon = moonModels[i];
                Vector3 position = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + model.solar.satelites[moon.solarIndex[1]].lastKnownPosition
                    + moon.GamePosition(game.data.date.time));

                Vector3 screenPoint = Camera.main.WorldToViewportPoint(position);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

                if (onScreen)
                {
                    LineRenderer line = moons[i].GetComponent<LineRenderer>();

                    moons[i].transform.position = position;

                    //Creates the line rendering for the orbit of moons

                    var positions = new Vector3[361];
                    double time = game.data.date.time;
                    moon.SetOrbit(time, model.solar.satelites[moon.solarIndex[1]].mass);

                    for (var b = 0; b < 360; b++)
                    {
                        positions[b] = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + model.solar.satelites[moon.solarIndex[1]].lastKnownPosition
                    + moon.approximatePositions[b]);
                    }
                    line.positionCount = 360;
                    line.SetPositions(positions);
                    yield return null;
                }

            }
            yield return null;
        }
    }

    private bool CheckVisibility(SolarBody body)
    {
        if (body.solarType == SolarType.Planet)
        {
            if (!MapTogglePanel.instance.planet.isOn)
            {
                return false;
            }
            else
            {
                return MapTogglePanel.instance.subtypes[body.solarSubType].isOn;
            }
        }
        if (body.solarType == SolarType.DwarfPlanet)
        {
            if (!MapTogglePanel.instance.dwarfPlanet.isOn)
            {
                return false;
            }
            else
            {
                return MapTogglePanel.instance.subtypes[body.solarSubType].isOn;
            }
        }
        if (body.solarType == SolarType.Comet)
        {
            if (!MapTogglePanel.instance.comet.isOn)
            {
                return false;
            }
            else
            {
                return MapTogglePanel.instance.subtypes[body.solarSubType].isOn;
            }
        }
        if (body.solarType == SolarType.Asteroid)
        {
            if (!MapTogglePanel.instance.asteroid.isOn)
            {
                return false;
            }
            else
            {
                return MapTogglePanel.instance.subtypes[body.solarSubType].isOn;
            }
        }

        if (body.solarType == SolarType.Moon)
        {
            if (!MapTogglePanel.instance.moons.isOn)
            {
                return false;
            }
            else if (planets[body.solarIndex[1]].activeSelf)
            {
                return MapTogglePanel.instance.subtypes[body.solarSubType].isOn;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    internal SolarModel GetModel()
    {
        return model;
    }

    protected override void OnModelChanged()
    {

        sprite.color = model.color;
        if (model.isActive)
            transform.localScale = Vector3.one * model.localScale;
    }

    public void ToggleSystem()
    {
        SystemToggle();
    }
    //TODO: new way to save and load focus star system
    private void SystemToggle()
    {
        if (galaxy.solarModel == model && Camera.main.cullingMask == galaxy.solarMask && !model.isActive)
        {
            model.isActive = true;
            game.nameOfSystem.text = model.name;
            var mainCam = Camera.main;
            var newCamPos = mainCam.transform.position;
            mainCam.transform.position = lastCamPosition;
            lastCamPosition = newCamPos;
            CreateSystem();
        }
        else if ((galaxy.solarModel != model || Camera.main.cullingMask != galaxy.solarMask) && model.isActive)
        {
            var mainCam = Camera.main;
            var newCamPos = mainCam.transform.position;
            mainCam.transform.position = lastCamPosition;
            lastCamPosition = newCamPos;
            model.isActive = false;
            game.nameOfSystem.text = game.data.galaxyName;
            DestroySystem();
        }
    }

    public void CreateSystem()
    {
        model.localScale = 1;
        transform.localScale = Vector3.one;
        sun = Instantiate(sunObj, transform);
        sun.name = model.name + " Sun";
        sun.transform.localPosition = Vector3.zero;
        sun.transform.localScale = Vector3.one * (float)(Mathd.Pow((model.solar.bodyRadius), solarSpriteScale) * Mathd.Pow(game.data.cameraGalCameraScaleMod, .9f));
        sun.GetComponent<SpriteRenderer>().color = model.solar.color;
        sun.GetComponent<SpriteRenderer>().sortingOrder = 5;

        var info = sun.GetComponent<PlanetInfo>();
        info.solar = model.solar;

        planets = new List<GameObject>();
        for (int i = 0; i < model.solar.satelites.Count; i++)
        {
            SolarBody body = model.solar.satelites[i];
            Vector3 position = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + body.GamePosition(game.data.date.time));
            planets.Add(Instantiate(planetObj, transform));
            planets[i].name = body.name;
            planets[i].transform.position = position;
            planets[i].GetComponent<SpriteRenderer>().color = body.color;
            planets[i].GetComponent<SpriteRenderer>().sortingOrder = 4;
            planets[i].transform.localScale = sun.transform.localScale * .5f;

            info = planets[i].GetComponent<PlanetInfo>();
            info.solar = body;

            LineRenderer line = planets[i].GetComponent<LineRenderer>();

            //Creates the line rendering for the orbit of planets

            Vector3[] positions = new Vector3[361];
            double time = game.data.date.time;
            body.SetOrbit(time, model.solar.mass);


            for (var b = 0; b < 360; b++)
            {
                positions[b] = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + body.approximatePositions[b]);
            }
            line.positionCount = 360;
            line.SetPositions(positions);


            if (body.solarSubType == SolarSubType.GasGiant)
            {
                Color col = Color.blue;
                col.a = .1f;
                line.startColor = col;
                line.endColor = col;
            }
            else if (body.solarType == SolarType.DwarfPlanet)
            {
                Color col = Color.yellow;
                col.a = .1f;
                line.startColor = col;
                line.endColor = col;
            }
            else if (body.solarType == SolarType.Comet)
            {
                Color col = Color.white;
                col.a = .1f;
                line.startColor = col;
                line.endColor = col;
            }
            else
            {
                Color col = Color.green;
                col.a = .1f;
                line.startColor = col;
                line.endColor = col;
            }

            //Create Moons
            for (int m = 0; m < model.solar.satelites[i].satelites.Count; m++)
            {
                SolarBody moon = model.solar.satelites[i].satelites[m];
                position = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + model.solar.satelites[moon.solarIndex[1]].lastKnownPosition
                    + moon.GamePosition(game.data.date.time));
                moonModels.Add(moon);

                moons.Add(Instantiate(moonObj, transform));
                moons[moons.Count - 1].name = moon.name;
                moons[moons.Count - 1].transform.position = position;
                moons[moons.Count - 1].GetComponent<SpriteRenderer>().color = moon.color;
                moons[moons.Count - 1].GetComponent<SpriteRenderer>().sortingOrder = 3;
                moons[moons.Count - 1].transform.localScale = sun.transform.localScale * .15f;

                info = moons[moons.Count - 1].GetComponent<PlanetInfo>();
                info.solar = moon;

                //Creates the line rendering for the orbit of moons

                positions = new Vector3[361];
                time = game.data.date.time;
                moon.SetOrbit(time, model.solar.satelites[moon.solarIndex[1]].mass);

                for (var b = 0; b < 360; b++)
                {
                    positions[b] = CameraController.CameraOffsetGalaxyPosition(model.galaxyPosition + model.solar.satelites[moon.solarIndex[1]].lastKnownPosition
                + moon.approximatePositions[b]);
                }
                line.positionCount = 360;
                line.SetPositions(positions);
            }
        }

        StartCoroutine("UpdateSolarObjects");
    }

    public void DestroySystem()
    {
        //if (model.nameText != "" && model.nameText != null)
        //{
        //    nameButton.enabled = false;
        //}
        StopAllCoroutines();
        transform.localScale = Vector3.one * (float) Mathd.Pow((model.solar.bodyRadius), .01f);
        model.localScale = (float) Mathd.Pow((model.solar.bodyRadius), .01f);
        Destroy(sun);
        for (int i = 0; i < planets.Count; i++)
        {
            Destroy(planets[i]);
            
        }
        planets = new List<GameObject>();
        for (int i = 0; i < moons.Count; i++)
        {
            Destroy(moons[i]);
            
        }
        moons = new List<GameObject>();
    }

    public void OnMouseEnter()
    {
        ToolTip.instance.SetTooltip(model.name, String.Format("Satellites: {0}", model.solar.satelites.Count));
    }
    public void OnMouseExit()
    {
        ToolTip.instance.CancelTooltip();
    }
}