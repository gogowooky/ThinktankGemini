using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ThinktankApp
{
    public class TTAction : TTObject
    {
        public object Script { get; set; }

        public TTAction() : base()
        {
            Script = null;
        }

        public bool Invoke(object tag, Runspace runspace)
        {
            try
            {
                // Ensure runspace is open
                if (runspace.RunspaceStateInfo.State != RunspaceState.Opened)
                {
                    if (runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen)
                        runspace.Open();
                    else
                        return false; // Cannot use closed/broken runspace
                }

                // Set Tag variable in the runspace
                runspace.SessionStateProxy.SetVariable("Tag", tag);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;
                    
                    if (Script is string)
                    {
                        ps.AddScript((string)Script);
                    }
                    else
                    {
                        // Assume ScriptBlock (PSObject or direct ScriptBlock)
                        ps.AddCommand("Invoke-Command")
                          .AddParameter("ScriptBlock", Script)
                          .AddParameter("ArgumentList", new object[] { tag }); // Pass tag as argument too if needed
                    }
                    
                    // Invoke the script
                    var results = ps.Invoke();

                    // Output streams to Console for debugging
                    if (ps.HadErrors)
                    {
                        foreach (var error in ps.Streams.Error)
                        {
                            System.Console.WriteLine("ERROR: " + error.ToString());
                        }
                    }
                    foreach (var warning in ps.Streams.Warning)
                    {
                        System.Console.WriteLine("WARNING: " + warning.ToString());
                    }
                    foreach (var info in ps.Streams.Information)
                    {
                        System.Console.WriteLine("INFO: " + info.ToString());
                    }
                    foreach (var verbose in ps.Streams.Verbose)
                    {
                        System.Console.WriteLine("VERBOSE: " + verbose.ToString());
                    }

                    if (results != null && results.Count > 0)
                    {
                        foreach (var result in results)
                        {
                            if (result != null && result.BaseObject is bool)
                            {
                                return (bool)result.BaseObject;
                            }
                        }
                    }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error invoking TTAction: " + ex.Message);
                return false;
            }
        }
    }
}
