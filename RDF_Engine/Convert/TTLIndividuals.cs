﻿using BH.oM.Base;
using BH.oM.RDF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.RDF
{
    public static partial class Convert
    {
        public static List<string> TTLIndividuals(this CSharpGraph cSharpGraph, LocalRepositorySettings localRepositorySettings)
        {
            List<string> TTLIndividuals = new List<string>();

            foreach (object individual in cSharpGraph.AllIndividuals)
            {
                if (individual == null)
                    continue;

                string TTLIndividual = "";

                string individualUri = individual.IndividualUri(cSharpGraph.OntologySettings).ToString();

                TTLIndividual += $"\n### {individualUri}";
                TTLIndividual += $"\n<{individualUri}> rdf:type owl:NamedIndividual ,";
                TTLIndividual += $"\n\t\t:{individual.IndividualType(cSharpGraph.OntologySettings.TBoxSettings).UniqueNodeId()} ;";

                TTLIndividual += TLLIndividualRelations(individual, cSharpGraph, localRepositorySettings);

                TTLIndividual = TTLIndividual.ReplaceLastOccurenceOf(';', ".");
                TTLIndividuals.Add(TTLIndividual);
            }

            return TTLIndividuals;
        }

        private static string TLLIndividualRelations(object individual, CSharpGraph cSharpGraph, LocalRepositorySettings localRepositorySettings)
        {
            string TLLIndividualRelations = "";
            List<IndividualRelation> individualRelations = cSharpGraph.IndividualRelations.Where(r => r.Individual == individual).ToList();

            foreach (IndividualRelation individualRelation in individualRelations)
            {
                IndividualObjectProperty iop = individualRelation as IndividualObjectProperty;
                if (iop != null)
                {
                    // First check if the Object Property is a List.
                    // This check is done here rather than at the CSharpGraph stage because not all output formats support lists.
                    // TTL supports lists.
                    if (iop.RangeIndividual.GetType().IsListOfOntologyClasses())
                    {
                        var individualList = iop.RangeIndividual as IEnumerable<object>;
                        if (individualList == null)
                            continue;

                        List<string> listIndividualsUris = individualList.Where(o => o != null).Select(o => o.IndividualUri(cSharpGraph.OntologySettings).ToString()).ToList();
                        TLLIndividualRelations += $"\n\t\t:{iop.HasProperty.PropertyInfo.UniqueNodeId()} rdf:seq ;\n";

                        for (int i = 0; i < listIndividualsUris.Count; i++)
                        {
                            string individualUri = listIndividualsUris[i];

                            TLLIndividualRelations += $"\t\trdf:_{i} <{individualUri}> ;\n";
                        }

                        // TODO: Verify how to handle empty lists.
                    }
                    else
                        TLLIndividualRelations += $"\n\t\t:{iop.HasProperty.PropertyInfo.UniqueNodeId()} <{iop.RangeIndividual.IndividualUri(cSharpGraph.OntologySettings)}> ;";

                    continue;
                }

                IndividualDataProperty idp = individualRelation as IndividualDataProperty;
                if (idp != null)
                {
                    TLLIndividualRelations += "\n\t\t" + $@":{idp.PropertyInfo.UniqueNodeId()} ""{idp.StringValue()}""";

                    string dataType = idp.Value.GetType().ToOntologyDataType();

                    if (dataType == typeof(Base64JsonSerialized).UniqueNodeId())
                        TLLIndividualRelations += $"^^:{ idp.Value.GetType().ToOntologyDataType()};";
                    else
                        TLLIndividualRelations += $"^^{ idp.Value.GetType().ToOntologyDataType()};"; // TODO: insert serialized value here, when the individual's datatype is unknown

                    continue;
                }
            }

            return TLLIndividualRelations;
        }
    }
}
