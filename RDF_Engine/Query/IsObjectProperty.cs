/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.RDF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Base;
using BH.Engine.Reflection;
using BH.oM.Base;

namespace BH.Engine.RDF
{
    public static partial class Query
    {
        public static bool IsObjectProperty(this PropertyInfo pi, TBoxSettings tBoxSettings)
        {
            // An ontology Object property is a relation between two classes of an ontology.
            // A CSharp PropertyInfo can corresponds to an Object Property if:
            //   1) The domain of the relation (the PropertyInfo's DeclaringType) can be translated to an Ontology class;
            //   2) The range of the relation (the PropertyInfo's PropertyType):
            //       2a) can be translated to an Ontology class, OR
            //       2b) is a List of objects, whether it contains objects translatable to Ontology class or DataTypes (https://github.com/BHoM/RDF_Prototypes/issues/17)

            return pi.PropertyType.IsOntologyClass(tBoxSettings) && pi.DeclaringType.IsOntologyClass(tBoxSettings) || pi.PropertyType.IsList();
        }
    }
}
