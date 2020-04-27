using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DragonCurve
{
    /// <summary>
    /// Dragon curve(alternative version)
    /// multi-threading component inherit from GH_TaskCapableComponent
    /// </summary>
    public class DragonCurveTaskComponent : GH_TaskCapableComponent<DragonCurveTaskComponent.SolveResults>
    {
        //Constructor
        public DragonCurveTaskComponent() : base("Dragon curve", "DC_t", "Mult-threading compute Dragon curve", "User", "Test")
        {
        }
        #region Input, Output
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Number of depth", "N", "The number of recursive depth level", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length of curves", "L", "The length of curves", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Dragon curve", "DragonCrv", "Create dragon curve", GH_ParamAccess.item);
            pManager.AddPointParameter("Curve points", "DragonCrvPts", "Create points of dragon curve", GH_ParamAccess.list);
        }
        #endregion

        public class SolveResults
        {
            public Point3d Value { get; set; }
        }

        /// <summary>
        /// This is the method that create points list for recursive process
        /// Create a Compute function that takes the input retrieve from IGH_DataAccess
        /// and returns an instance of SolveResults
        /// </summary>
        private static SolveResults ComputeDragonPts(double length, int num)
        {
            var result = new SolveResults();

            
            return result;
        }

        //Generate GrowString from rules
        private void GrowString(ref int num, ref string finalString, string ruleX, string ruleY)
        {
            //Decrement the count with each new execution of the grow funcution.
            num = num - 1;
            char rule;

            //Create new string
            string newString = "";
            for (int i = 0; i < finalString.Length; i++)
            {
                rule = finalString[i];
                if (rule == 'X')
                {
                    newString = newString + ruleX;
                }
                if (rule == 'Y')
                {
                    newString = newString + ruleY;
                }
                if (rule == 'F' | rule == '+' | rule == '-')
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

        private void ParceDragonString(string dragonString, double length, ref List<Point3d> dragonPoints)
        {
            //parce instruction string to generate points
            //let base point be world origin
            var pt = Point3d.Origin;
            dragonPoints.Add(pt);

            //drawing direction vector - start along the x-axis
            //vector direction will be rotated depending on (+, -)instruction.
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
                //rotate Right
                if(rule == '-')
                {
                    vec.Rotate(-Math.PI / 2, Vector3d.ZAxis);
                }
            }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Declare placeholder valuable for the input data
            double length = double.NaN;
            int num = 2;
            
            //Retrieve input data
            if(!DA.GetData(0, ref num)) { return; }
            if(!DA.GetData(1, ref length)) { return; }

            if (InPreSolve)
            {
                //Queue up the task
                Task<SolveResults> task = Task.Run(() => ComputeDragonPts(length, num));
                TaskList.Add(task);

                return;
            }

            if(!GetSolveResults(DA, out SolveResults result))
            {
                //Compute results on a given data
                result = ComputeDragonPts(length, num);
            }

            //Set out put data
            if(result != null)
            {
                DA.SetData(0, new PolylineCurve(result.Value));
                DA.SetDataList(1, result.Value);
            }
            
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;

        public override Guid ComponentGuid => new Guid("{68269F36-29E2-45F1-8A7E-707CF60BBF37}");
    }
}
