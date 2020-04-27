using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { __out.Add(text); }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private IGH_Component Component; 
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments, 
  /// Output parameters as ref arguments. You don't have to assign output parameters, 
  /// they will have a default value.
  /// </summary>
  private void RunScript(string startString, string ruleX, string ruleY, int num, double length, ref object DragonCurve)
  {
        //startString = "FX"
    //ruleX = "-FX-Y", "X+YF+"
    //ruleY = "-FX-Y"
    
    //X : "X+YF+"
    //Y : "-FX-Y"
    //+ : "Rotate 90"
    //- : Rotate -90
    //F : Draw Forward
    
    
    //declare string
    var dragonString = startString;
    //generate the string
    GrowString(ref num, ref dragonString, ruleX, ruleY);
    //generate the points
    var dragonPts = new List<Point3d>();
    ParseDeagonString(dragonString, length, ref dragonPts);
    //create the curve
    var dragonCrv = new PolylineCurve(dragonPts);

    //Assign output
    DragonCurve = dragonCrv;
  }

  // <Custom additional code> 
  
  void GrowString(ref int num, ref string finalString, string ruleX, string ruleY)
  {
    //decrement the count with each new execution of the grow function
    num -= 1;
    char rule;
    //create new string
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
    if(num == 0)
    {
      return;
    }

    //Grow again
    GrowString(ref num, ref finalString, ruleX, ruleY);
  }

  void ParseDeagonString(string dragonString, double length, ref List<Point3d> dragonPoints)
  {
    //parse instruction string to generate points
    //let base point be world origin
    var pt = Point3d.Origin;
    dragonPoints.Add(pt);

    //drawing direction vector - start along the x-axis
    //vector direction will be rotated depending on (+, -) instructions
    var V = new Vector3d(1.0, 0.0, 0.0);

    char rule;
    for(int i = 0; i < dragonString.Length; i++)
    {
      //always start for 1 and length 1 to get one char at a time
      rule = dragonString[i];
      //move Forward using direction vector
      if(rule == 'F')
      {
        pt = pt + (V * length);
        dragonPoints.Add(pt);
      }
      //rotate Left
      if(rule == '+')
      {
        V.Rotate(Math.PI / 2, Vector3d.ZAxis);
      }
      //rotate Right
      if(rule == '-')
      {
        V.Rotate(-Math.PI / 2, Vector3d.ZAxis);
      }
    }
  }

  // </Custom additional code> 

  private List<string> __err = new List<string>(); //Do not modify this list directly.
  private List<string> __out = new List<string>(); //Do not modify this list directly.
  private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
  private IGH_ActiveObject owner;                  //Legacy field.
  private int runCount;                            //Legacy field.
  
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
  {
    //Prepare for a new run...
    //1. Reset lists
    this.__out.Clear();
    this.__err.Clear();

    this.Component = owner;
    this.Iteration = iteration;
    this.GrasshopperDocument = owner.OnPingDocument();
    this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

    this.owner = this.Component;
    this.runCount = this.Iteration;
    this. doc = this.RhinoDocument;

    //2. Assign input parameters
        string startString = default(string);
    if (inputs[0] != null)
    {
      startString = (string)(inputs[0]);
    }

    string ruleX = default(string);
    if (inputs[1] != null)
    {
      ruleX = (string)(inputs[1]);
    }

    string ruleY = default(string);
    if (inputs[2] != null)
    {
      ruleY = (string)(inputs[2]);
    }

    int num = default(int);
    if (inputs[3] != null)
    {
      num = (int)(inputs[3]);
    }

    double length = default(double);
    if (inputs[4] != null)
    {
      length = (double)(inputs[4]);
    }



    //3. Declare output parameters
      object DragonCurve = null;


    //4. Invoke RunScript
    RunScript(startString, ruleX, ruleY, num, length, ref DragonCurve);
      
    try
    {
      //5. Assign output parameters to component...
            if (DragonCurve != null)
      {
        if (GH_Format.TreatAsCollection(DragonCurve))
        {
          IEnumerable __enum_DragonCurve = (IEnumerable)(DragonCurve);
          DA.SetDataList(1, __enum_DragonCurve);
        }
        else
        {
          if (DragonCurve is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(DragonCurve));
          }
          else
          {
            //assign direct
            DA.SetData(1, DragonCurve);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }

    }
    catch (Exception ex)
    {
      this.__err.Add(string.Format("Script exception: {0}", ex.Message));
    }
    finally
    {
      //Add errors and messages... 
      if (owner.Params.Output.Count > 0)
      {
        if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
        {
          List<string> __errors_plus_messages = new List<string>();
          if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
          if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
          if (__errors_plus_messages.Count > 0) 
            DA.SetDataList(0, __errors_plus_messages);
        }
      }
    }
  }
}