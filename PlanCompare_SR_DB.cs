using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using PlanCompare_SR_DB;
using PlanCompare_SR_DB.Views;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS {
    public class Script {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]

        public void Execute(ScriptContext context, Window mainWindow)
        {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            Run(context.CurrentUser, context.Patient, context.Image, context.StructureSet, context.PlanSetup, context.PlansInScope, context.PlanSumsInScope, mainWindow);

            //Code for PlugIn app.  Commented out for ScritpRunner app.
            //mainWindow wnd = new mainWindow();

            //wnd.DataContext = context;
            //wnd.theContext = context;

            //wnd.ShowDialog();

        }

        public void Run( User user, Patient patient, Image image, StructureSet structureSet, PlanSetup planSetup,
        IEnumerable<PlanSetup> planSetupsInScope, IEnumerable<PlanSum> planSumsInScope, Window mainWindow)
        {
            // Your main code now goes here

            //Use this version for PlugIn version
            //mainWindowContents wndContents = new mainWindowContents();

            //Use this for ScriptRunner version
            mainWindowContents wndContents = new mainWindowContents(patient, planSetup);

            mainWindow.Width = wndContents.Width;
            mainWindow.Height = wndContents.Height;
            mainWindow.Content = wndContents;

            //Show the main window.
            mainWindow.ShowDialog();
            
        }

    }
}
