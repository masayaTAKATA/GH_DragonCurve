using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

/// <summary>
/// 
/// </summary>

namespace DragonCurve
{
    public class DragonCurveComponent : GH_Component
    {
        public DragonCurveComponent() : base("DragonCurve", "DC", "Standard compute DragonCurve","Meenaxy", "Default")
        {
        }

        #region Input, Output
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Number of depth", "N", "The number of recursive depth level", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length of curves", "L", "The length of curves", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Dragon curve", "DragonCrv", "Create dragon curve", GH_ParamAccess.item);
            pManager.AddPointParameter("Curve points", "DragonCrvPts", "Create points of dragon curve", GH_ParamAccess.list);
        }
        #endregion

        /// <summary> Rule Strings
        /// startString = "FX"
        ///ruleX = "-FX-Y", "X+YF+"
        ///ruleY = "-FX-Y"
        ///
        ///X : "X+YF+"
        ///Y : "-FX-Y"
        ///+ : "Rotate 90"
        ///- : Rotate -90
        ///F : Draw Forward
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Declare start and rule string(example)
            string startString = "FX";
            string ruleX = "X+YF+";
            string ruleY = "-FX-Y";

            int num = 2;
            double length = double.NaN;
            
            if(!DA.GetData(0, ref num)) { return; }
            if(!DA.GetData(1, ref length)) { return; }

            //we should now validate the data and warn the user if invalid data is supplied.
            if(num <= 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of depth must be bigger than One.");
                return;
            }
            if(length < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Length must be bigger than Zero.");
                return;
            }
            if(num > 10)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Number of depth is recommended under Ten");
                return;
            }

            //Declare a string
            var dragonString = startString;
            //Generate the string.
            GrowString(ref num, ref dragonString, ruleX, ruleY);
            //Generate the points
            var dragonPts = new List<Point3d>();
            ParceDragonString(dragonString, length, ref dragonPts);
            //Create the curve
            var dragonCrv = new PolylineCurve(dragonPts);

            //Assign output
            DA.SetData(0, dragonCrv);
            DA.SetDataList(1, dragonPts);
        }

        //Generate GrowString from rules
        private void GrowString(ref int num, ref string finalString, string ruleX, string ruleY)
        {
            //Decrement the count with each new execution of the grow funcution.
            num -= 1;
            char rule;

            //Create new string
            string newString = "";
            for(int i = 0; i < finalString.Length; i++)
            {
                rule = finalString[i];
                if(rule == 'X')
                {
                    newString = newString + ruleX;
                }
                if(rule == 'Y')
                {
                    newString = newString + ruleY;
                }
                if(rule == 'F' | rule == '+' | rule == '-')
                {
                    newString = newString + rule;
                }
            }
            finalString = newString;

            //Stop condition
            if (num == 0)
            {
                return;
            }

            //Grow again(recursive)
            GrowString(ref num, ref finalString, ruleX, ruleY);

        }

        //Dragon curves is generate from the dragonString
        private void ParceDragonString(string dragonString, double length, ref List<Point3d> dragonPoints)
        {
            //parce instruction string to generate points
            //let base point be world origin
            var pt = Point3d.Origin;
            dragonPoints.Add(pt);

            //drawing direction vector - start along the x-axis
            //vector direction will be rotated depending on (+, -) instruction.
            var vec = new Vector3d(1.0, 0.0, 0.0);

            char rule;
            for(int i = 0; i < dragonString.Length; i++)
            {
                //always start for 1 and length 1 to get one char at a time.
                rule = dragonString[i];
                //move Forward using direction vector.
                if(rule == 'F')
                {
                    pt = pt + (vec * length);
                    dragonPoints.Add(pt);
                }
                //rotate Left
                if(rule == '+')
                {
                    vec.Rotate(Math.PI / 2, Vector3d.ZAxis);
                }
                //rotate Right.
                if(rule == '-')
                {
                    vec.Rotate(-Math.PI / 2, Vector3d.ZAxis);
                }
            }
        }

        /// <summary>
        /// component icon image data
        /// https://www.flaticon.com/free-icon/japanese-dragon_12777
        /// </summary>
        protected override System.Drawing.Bitmap Icon => DragonCurve.Properties.Resources.japanese_dragon;

        public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;

        public override Guid ComponentGuid => new Guid("{3ACABAC8-632C-4CB5-ABDF-EC3BE35BEC0B}");
    }
}
