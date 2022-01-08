using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Drawing;

/**
Program Name: Oscillator synchronization
Author: Nicolas Vargas
Date completed: 1/8/2022

Special Thanks to Jose Luis from Parametric Camp for providing public
lectures/tutorials on modeling a christmas tree using Rhino and Grasshopper.

Link the Parametric Camp youtube channel: https://www.youtube.com/channel/UCSgG9KzVsS6jArapCx-Bslg
**/


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
  private void RunScript(List<Point3d> leds, int fps, int duration, int oscillators, double k, ref object CSV)
  {
        // Count the number of LEDs on the tree
    int ledCount = leds.Count;

    // Prepare the first row of the CSV file
    List<string> csv = new List<string>();

    // Add the header, which looks like:
    // "FRAME_ID,R_0,G_0,B_0,R_1,G_1,B_1..."
    string row = "FRAME_ID,";
    for (int i = 0; i < ledCount; i++)
    {
      row += "R_" + i + ",G_" + i + ",B_" + i;
      if (i != ledCount - 1)
      {
        row += ",";
      }
    }
    csv.Add(row);




    // Total length of animation (# of frames) for one conic section
    int frameCount = fps * duration;

    //Find the highest and lowest LED
    double minZ = double.MaxValue;
    double maxZ = double.MinValue;
    for (int i = 0; i < ledCount; i++)
    {
      if (leds[i].Z > maxZ) maxZ = leds[i].Z;
      if (leds[i].Z < minZ) minZ = leds[i].Z;
    }
    double treeHeight = maxZ - minZ;
    double amplitude = treeHeight / oscillators;
    double[] phases = new double[oscillators];
    for(int i = 0; i < oscillators; i++)
    {
      phases[i] = i * 2;
    }


    

    // Generate rows for each frame
    for (int f = 0; f < frameCount; f++)
    {
      row = f + ",";
      // Get the adjusted phase of each segment for 
      // the next frame using the Kuramoto model
      double[] newPhases = kuramotoSum(phases, k);

      // Calculate the values for each LED
      int r = 0, g = 0, b = 0;
      for (int i = 0; i < ledCount; i++)
      {
        // Evaluate what segment affects the LED
        for(int j = 0; j < oscillators; j++)
        {
          if(leds[i].Z > (amplitude * j) && leds[i].Z < (amplitude * (j + 1)))
          {
            double ledToPlane = Math.Sin(2 * leds[i].Y + 0.05 * f + newPhases[j]);
            if (ledToPlane < 0)
            {
              r = 30 * (int) Math.Round((-1 * Math.Exp(-30 * Math.Pow((ledToPlane), 2)) + 1));
              g = 90 * (int) Math.Round(Math.Exp(-30 * Math.Pow((ledToPlane), 2)));
              b = 90 * (int) Math.Round(Math.Exp(-30 * Math.Pow((ledToPlane), 2)));
            }
            else
            {
              r = 30 * (int) Math.Round((-1 * Math.Exp(-30 * Math.Pow((ledToPlane), 2)) + 1));
              g = 90 * (int) Math.Round(Math.Exp(-30 * Math.Pow((ledToPlane), 2)));
              b = 90 * (int) Math.Round(Math.Exp(-30 * Math.Pow((ledToPlane), 2)));
            }
          }
        }

        // Add these colors to the row
        row += r + "," + g + "," + b;
        if (i != ledCount - 1)
        {
          row += ",";
        }
      }

      phases = newPhases;
      Print(phases[0].ToString());

      // Add this row to the CSV
      csv.Add(row);
    }


    // Outputs
    CSV = csv;

  }

  // <Custom additional code> 
    // Calculate the phase of each segment for the next frame using the Kuramoto model
  public double[] kuramotoSum(double[] phases, double k)
  {
    int amountOfPhases = phases.Length;
    double[] newPhases = phases;

    int n = amountOfPhases;

    for (int i = 0; i < n; i++)
    {
      for(int j = 0; j < n; j++)
      {
        //when j = i, sin is 0.0 and does not affect the sum
        newPhases[i] += k * Math.Sin(phases[j] - phases[i]);
      }
    }
    return newPhases;
  }




  // From https://stackoverflow.com/a/1626232/1934487
  public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
  {
    int max = Math.Max(color.R, Math.Max(color.G, color.B));
    int min = Math.Min(color.R, Math.Min(color.G, color.B));

    hue = color.GetHue();
    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
    value = max / 255d;
  }

  // From https://stackoverflow.com/a/1626232/1934487
  public static Color ColorFromHSV(double hue, double saturation, double value)
  {
    int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
    double f = hue / 60 - Math.Floor(hue / 60);

    value = value * 255;
    int v = Convert.ToInt32(value);
    int p = Convert.ToInt32(value * (1 - saturation));
    int q = Convert.ToInt32(value * (1 - f * saturation));
    int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

    if (hi == 0)
      return Color.FromArgb(255, v, t, p);
    else if (hi == 1)
      return Color.FromArgb(255, q, v, p);
    else if (hi == 2)
      return Color.FromArgb(255, p, v, t);
    else if (hi == 3)
      return Color.FromArgb(255, p, q, v);
    else if (hi == 4)
      return Color.FromArgb(255, t, p, v);
    else
      return Color.FromArgb(255, v, p, q);
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
        List<Point3d> leds = null;
    if (inputs[0] != null)
    {
      leds = GH_DirtyCaster.CastToList<Point3d>(inputs[0]);
    }
    int fps = default(int);
    if (inputs[1] != null)
    {
      fps = (int)(inputs[1]);
    }

    int duration = default(int);
    if (inputs[2] != null)
    {
      duration = (int)(inputs[2]);
    }

    int oscillators = default(int);
    if (inputs[3] != null)
    {
      oscillators = (int)(inputs[3]);
    }

    double k = default(double);
    if (inputs[4] != null)
    {
      k = (double)(inputs[4]);
    }



    //3. Declare output parameters
      object CSV = null;


    //4. Invoke RunScript
    RunScript(leds, fps, duration, oscillators, k, ref CSV);
      
    try
    {
      //5. Assign output parameters to component...
            if (CSV != null)
      {
        if (GH_Format.TreatAsCollection(CSV))
        {
          IEnumerable __enum_CSV = (IEnumerable)(CSV);
          DA.SetDataList(1, __enum_CSV);
        }
        else
        {
          if (CSV is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(CSV));
          }
          else
          {
            //assign direct
            DA.SetData(1, CSV);
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