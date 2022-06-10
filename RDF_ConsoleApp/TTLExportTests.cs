﻿using BH.oM.Architecture.Elements;
using System;
using System.Collections.Generic;
using BH.oM.Geometry;
using BH.Engine.RDF;
using BH.oM.RDF;
using BH.oM.Physical.Elements;
using BH.oM.Base;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace BH.Test.RDF
{
    public static class TTLExportTests
    {
        private static OntologySettings m_shortAddresses = new OntologySettings()
        {
            ABoxSettings = new ABoxSettings() { IndividualsBaseAddress = "individuals.Address" },
            TBoxSettings = new TBoxSettings() { CustomTypesBaseAddress = "customTypes.Address" }
        };

        public static string Room()
        {
            Room room = new Room();
            room.Perimeter = new Polyline() { ControlPoints = new List<Point>() { new Point(), new Point() { X = 5, Y = 5, Z = 5 }, new Point() { X = 99 } } };
            room.Location = new Point();
            room.Name = "A room object";

            List<IObject> objectList = new List<IObject>() { room };
            string TTLGraph = objectList.TTLGraph(new OntologySettings(), new LocalRepositorySettings());

            Console.Write(TTLGraph);

            Graph g = new Graph();
            StringParser.Parse(g, TTLGraph);

            return TTLGraph;
        }

        public static string Column()
        {
            Column randomColumn = BH.Engine.RDF.Testing.Create.RandomObject<Column>();

            List<IObject> objectList = new List<IObject>() { randomColumn };
            string TTLGraph = objectList.TTLGraph(new OntologySettings(), new LocalRepositorySettings());

            Console.Write(TTLGraph);

            Graph g = new Graph();
            StringParser.Parse(g, TTLGraph);

            return TTLGraph;
        }

        public static string RoomAndColumn()
        {
            Room room = new Room();
            room.Perimeter = new Polyline() { ControlPoints = new List<Point>() { new Point(), new Point() { X = 5, Y = 5, Z = 5 }, new Point() { X = 99 } } };
            room.Location = new Point();
            room.Name = "A room object";

            Column randomColumn = BH.Engine.RDF.Testing.Create.RandomObject<Column>();

            List<IObject> objectList = new List<IObject>() { room, randomColumn };
            string TTLGraph = objectList.TTLGraph(new OntologySettings(), new LocalRepositorySettings());

            Console.Write(TTLGraph);

            Graph g = new Graph();
            StringParser.Parse(g, TTLGraph);

            return TTLGraph;
        }

        public static string CustomObject()
        {
            CustomObject customObject = BH.Engine.Base.Create.CustomObject(new Dictionary<string, object>() { { "Type", "Cassette" },
                { "intProperty", 10 },
                { "pointProperty", BH.Engine.RDF.Testing.Create.RandomObject<Point>() } });

            CSharpGraph cSharpGraph_customObj = Compute.CSharpGraph(new List<IObject>() { customObject }, m_shortAddresses);
            string TTLGraph = cSharpGraph_customObj.ToTTLGraph(new LocalRepositorySettings());

            Console.Write(TTLGraph);

            Graph g = new Graph();
            StringParser.Parse(g, TTLGraph);

            return TTLGraph;
        }

        public static void CustomObject_SameType_Error()
        {
            CustomObject customObject1 = BH.Engine.Base.Create.CustomObject(new Dictionary<string, object>() { { "Type", "Cassette" } });
            CustomObject customObject2 = BH.Engine.Base.Create.CustomObject(new Dictionary<string, object>() { { "Type", "Cassette" } });

            try
            {
                Compute.CSharpGraph(new List<IObject>() { customObject1, customObject2 }, m_shortAddresses);
            }
            catch (ArgumentException e)
            {
                return;
            }

            throw new Exception("Exception should have been thrown before.");
        }

        public static string Lists()
        {
            NurbsCurve nurbs = new NurbsCurve()
            {
                ControlPoints = new List<Point>()
                {
                    new Point(),
                    new Point() { X = 1, Y = 1, Z = 1 },
                    new Point() { X = 2, Y = 2, Z = 2 }
                }
            };

            CSharpGraph cSharpGraph_customObj = Compute.CSharpGraph(new List<IObject>() { nurbs }, m_shortAddresses);
            string TTLGraph = cSharpGraph_customObj.ToTTLGraph(new LocalRepositorySettings());

            return TTLGraph;
        }
    }
}
