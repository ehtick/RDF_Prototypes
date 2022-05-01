﻿
using BH.Engine.Base;
using BH.oM.Base;
using BH.oM.RDF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log = BH.oM.RDF.Log;

namespace BH.Engine.RDF
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Convert a Graph ontological representation of (BHoM) types and their relations into a Json format readable by WebVOWL (http://vowl.visualdataweb.org/webvowl.html).")]
        public static string ToTTLGraph(this CSharpGraph cSharpGraph, LocalRepositorySettings localRepositorySettings = null)
        {
            localRepositorySettings = localRepositorySettings ?? new LocalRepositorySettings();

            string TTL = Query.TTLHeader(cSharpGraph.OntologySettings.OntologyTitle, cSharpGraph.OntologySettings.OntologyDescription, cSharpGraph.OntologySettings.OntologyBaseAddress);

            TTL += Query.TTLSectionTitle("Annotation Properties");
            TTL += string.Join("\n", TTLAnnotationProperties());

            TTL += Query.TTLSectionTitle("Datatypes");
            TTL += string.Join("\n", TTLDataTypes(cSharpGraph, localRepositorySettings));

            TTL += Query.TTLSectionTitle("Classes");
            TTL += string.Join("\n\n", TTLClasses(cSharpGraph, localRepositorySettings));

            TTL += Query.TTLSectionTitle("Object Properties");
            TTL += string.Join("\n\n", TTLObjectProperties(cSharpGraph, localRepositorySettings));

            TTL += Query.TTLSectionTitle("Data properties");
            TTL += string.Join("\n\n", TTLDataProperties(cSharpGraph, localRepositorySettings));

            if (cSharpGraph.AllIndividuals?.Any() ?? false)
            {
                TTL += Query.TTLSectionTitle("Individuals");
                TTL += string.Join("\n\n", TTLIndividuals(cSharpGraph, localRepositorySettings));
            }

            return TTL;
        }

        private static List<string> TTLAnnotationProperties()
        {
            List<string> annotationProperties = new List<string>();

            annotationProperties.Add("###  http://purl.org/dc/elements/1.1/#description\n<http://purl.org/dc/elements/1.1/#description> rdf:type owl:AnnotationProperty .");
            annotationProperties.Add("###  http://purl.org/dc/elements/1.1/#title\n<http://purl.org/dc/elements/1.1/#title> rdf:type owl:AnnotationProperty .");

            return annotationProperties;
        }

        private static List<string> TTLDataTypes(CSharpGraph cSharpGraph, LocalRepositorySettings r)
        {
            OntologySettings s = cSharpGraph.OntologySettings;

            List<string> dataTypes = new List<string>();

            dataTypes.Add(Query.TTLDataType_DefaultTypeForUnknownConversion(r));

            return dataTypes;
        }

        private static List<string> TTLClasses(CSharpGraph cSharpGraph, LocalRepositorySettings localRepositorySettings)
        {
            List<string> TTLClasses = new List<string>();

            foreach (var t in cSharpGraph.Classes)
            {
                string TTLClass = "";

                // Declaration with Uri
                string typeUri = t.GithubURI(localRepositorySettings).ToString();
                TTLClass += $"### {typeUri}";

                // Class Identifier
                TTLClass += $"\n:{t.UniqueNodeId()} rdf:type owl:Class";

                // Subclasses
                var subClasses = cSharpGraph.ClassRelations.OfType<SubClassOfRelation>().Where(r => r.DomainClass == t).Select(r => r.RangeClass);
                foreach (var subClass in subClasses)
                {
                    TTLClass += $"\n\t\trdfs:subClassOf {subClass.UniqueNodeId()}";
                }

                // Class label
                TTLClass += "\n\t\t" + $@"rdfs:label""{t.DescriptiveName()}""@en .";

                TTLClasses.Add(TTLClass);
            }

            return TTLClasses;
        }

        private static List<string> TTLObjectProperties(CSharpGraph cSharpGraph, LocalRepositorySettings localRepositorySettings)
        {
            List<string> TTLObjectProperties = new List<string>();

            var hasPropertyRelation = cSharpGraph.ClassRelations.OfType<ObjectProperty>();

            foreach (var rel in hasPropertyRelation)
            {
                string TTLObjectProperty = "";

                string propertyURI = rel.PropertyInfo.GithubURI(localRepositorySettings).ToString();
                TTLObjectProperty += $"\n### {propertyURI}";
                TTLObjectProperty += $"\n:{rel.PropertyInfo.UniqueNodeId()} rdf:type owl:ObjectProperty ;";
                TTLObjectProperty += $"\nrdfs:domain {rel.DomainClass.UniqueNodeId()} ;";
                TTLObjectProperty += $"\nrdfs:range {rel.RangeClass.UniqueNodeId()} ;";
                TTLObjectProperty += "\n" + $@"rdfs:label ""{rel.PropertyInfo.DescriptiveName()}""@en .";

                TTLObjectProperties.Add(TTLObjectProperty);
            }

            return TTLObjectProperties;
        }

        private static List<string> TTLDataProperties(CSharpGraph cSharpGraph, LocalRepositorySettings localRepositorySettings)
        {
            List<string> TTLDataProperties = new List<string>();

            var hasPropertyRelation = cSharpGraph.ClassRelations.OfType<DataProperty>();

            foreach (var rel in hasPropertyRelation)
            {
                string TTLDataProperty = "";

                string propertyURI = rel.PropertyInfo.GithubURI(localRepositorySettings).ToString();
                TTLDataProperty += $"\n### {propertyURI}";
                TTLDataProperty += $"\n:{rel.PropertyInfo.UniqueNodeId()} rdf:type owl:ObjectProperty ;";
                TTLDataProperty += $"\nrdfs:domain {rel.DomainClass.UniqueNodeId()} ;";

                // We need to map the Range Type to a valid DataType.
                string dataType = rel.RangeType.ToDataType();

                TTLDataProperty += $"\nrdfs:range {dataType} ;";

                TTLDataProperty += "\n" + $@"rdfs:label ""{rel.PropertyInfo.DescriptiveName()}""@en .";

                TTLDataProperties.Add(TTLDataProperty);
            }

            // Add the IObject's DataProperty for the Default Data Type
            Type defaultTypeForUnknownConversions = typeof(JsonSerialized);
            string defaultDataType_IObjectProperty = "\n" + $@"###  {defaultTypeForUnknownConversions.GithubURI(localRepositorySettings)}
                {defaultTypeForUnknownConversions.DescriptiveName()} rdf:type owl:DatatypeProperty ;
                rdfs:domain :IObject ;
                rdfs:range {defaultTypeForUnknownConversions.UniqueNodeId()} .";

            return TTLDataProperties;
        }

        private static List<string> TTLIndividuals(CSharpGraph cSharpGraph, LocalRepositorySettings localRepositorySettings)
        {
            List<string> TTLIndividuals = new List<string>();

            foreach (object individual in cSharpGraph.AllIndividuals)
            {
                string TTLIndividual = "";

                string individualId = IndividualId(individual);
                string individualUri = $"{ cSharpGraph.OntologySettings.ABoxSettings.IndividualsBaseAddress }/{individualId}";

                TTLIndividual += $"\n### {individualUri}";
                TTLIndividual += $"\n<{individualUri}> rdf:type owl:NamedIndividual ,";
                TTLIndividual += $"\n\t\t{individual.GetType().UniqueNodeId()} ;";

                IEnumerable<IndividualRelation> individualRelations = cSharpGraph.IndividualRelations.Where(r => r.Individual == individual);

                foreach (IndividualRelation individualRelation in individualRelations)
                {
                    IndividualObjectProperty iop = individualRelation as IndividualObjectProperty;
                    IndividualDataProperty idp = individualRelation as IndividualDataProperty;

                    if (iop != null)
                    {
                        TTLIndividual += $"\n\t\t:{iop.HasProperty.PropertyInfo.UniqueNodeId()} {iop.RangeIndividual.IndividualId()} ;";
                    }
                    else if (idp != null)
                    {
                        TTLIndividual += "\n\t\t" + $@":{idp.PropertyInfo.UniqueNodeId()} ""{idp.GetStringValue()}"" ;"; // TODO: insert serialized value here, when the individual's datatype is unknown
                    }
                }

                TTLIndividual.ReplaceLastOccurenceOf(';', ".");

                TTLIndividuals.Add(TTLIndividual);
            }

            return TTLIndividuals;
        }

        private static string IndividualId(this object individual)
        {
            string individualId;

            IBHoMObject bHoMObject = individual as IBHoMObject;
            if (bHoMObject != null)
                return bHoMObject.BHoM_Guid.ToString();

            // TODO: What do we do when an individual does not have a Guid assigned?
            // We could take its Hash, but that is not unique/repeatable. 
            individualId = individual.GetHashCode().ToString();

            return individualId;
        }

        private static string GetStringValue(this IndividualDataProperty idp)
        {
            Type individualObjectType = idp.Value.GetType();
            if (ToOntologyDataType.ContainsKey(individualObjectType))
                return idp.Value.ToString(); // we can just return the ToString()

            // We must use our fallback for unknown conversions, serializing to Json.
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string serializedValue = JsonConvert.SerializeObject(idp.Value, settings);

            return serializedValue;
        }

        private static bool IsKnownDataType(Type t)
        {
            return ToOntologyDataType.ContainsKey(t);
        }

        private static string ToDataType(this Type t)
        {
            string ontologyDataType = null;
            ToOntologyDataType.TryGetValue(t, out ontologyDataType);

            if (ontologyDataType != null)
                return ontologyDataType;

            // Fallback
            return typeof(JsonSerialized).UniqueNodeId(); // assumes that the Data Type is already added to the Graph under this Identifier.
        }

        private static Dictionary<Type, string> ToOntologyDataType = new Dictionary<Type, string>()
        {
            { typeof(string), "xsd:string" },
            { typeof(bool), "xsd:boolean" },
            { typeof(int), "xsd:integer" },
            { typeof(float), "xsd:float" },
            { typeof(decimal), "xsd:decimal" },
            { typeof(Guid), "xsd:string"}
        };
    }
}
