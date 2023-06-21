using System.Linq;
 


namespace JiraCon
{
    public class MenuIssueStates : IMenuConsole
    {
        public JTISConfig ActiveConfig {get;set;}
        public MenuIssueStates(JTISConfig cfg)
        {
            ActiveConfig = cfg;                        
        }

        public void BuildMenu()
        {

            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();

            lines.AddConsoleLine(" ----------------------- " + padd, ConsoleUtil.StdStyle(StdLine.slMenuName));
            lines.AddConsoleLine("|  Time in Status Menu  |" + " " + cfgName, ConsoleUtil.StdStyle(StdLine.slMenuName));
            lines.AddConsoleLine(" ----------------------- " + padd, ConsoleUtil.StdStyle(StdLine.slMenuName));

//            lines.AddConsoleLine("-- [SHOW STATUS SELECTION & CLASSIFICATION] -- ", StdLine.slOutputTitle );
            lines.AddConsoleLine("(A) Edit Analysis Configuration", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine(string.Format("-- You have {0:00} Saved JQL Searches -- ",ActiveConfig.SavedJQLCount  ), ConsoleUtil.StdStyle(StdLine.slCode));
            lines.AddConsoleLine("(M) Manage Saved JQL", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(S) Manage Issue Status Classification", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("-------------------------------   ", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(V) View Summary for 1 Issue", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(I) Get Data for 1 or more Issue(s)", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(E) Get data for Issues in an Epic", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(J) Get data for Issues from JQL Query", ConsoleUtil.StdStyle(StdLine.slMenuDetail));

            lines.AddConsoleLine("",ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(B) Back to Main Menu", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("Enter selection or (X) to exit.", ConsoleUtil.StdStyle(StdLine.slResponse ));

            lines.WriteQueuedLines(true,true);
            lines = null;
        }

        public bool DoMenu()
        {
            BuildMenu();
            var resp = Console.ReadKey(true);
            return ProcessKey(resp.Key);
        }

        public bool ProcessKey(ConsoleKey key)
        {
//            ConsoleKeyInfo resp = default(ConsoleKeyInfo);


            if (key == ConsoleKey.B)
            {
                return false;
            }
            else if (key == ConsoleKey.X)
            {                
                if(ConsoleUtil.ByeBye())
                {
                    Environment.Exit(0);
                }
            }
            else if (key == ConsoleKey.S)
            {                
                while (MenuManager.DoMenu(new MenuStatusConfig(ActiveConfig)))
                {

                }
                return true;                
            }
            else if (key == ConsoleKey.M)
            {
                while (MenuManager.DoMenu(new MenuJQL(ActiveConfig)))
                {

                }
                return true;                
            }
            else if (key == ConsoleKey.V)
            {
                NewAnalysis(AnalysisType.atIssueSummary);
                return true;
            }
            else if (key == ConsoleKey.I)
            {
                NewAnalysis(AnalysisType.atIssues);
                return true;
            }
            else if (key == ConsoleKey.E)
            {
                NewAnalysis(AnalysisType.atEpics);
                return true;

            }
            else if (key == ConsoleKey.J)
            {
                NewAnalysis(AnalysisType.atJQL);
                return true;
            }
 

            return true;
        }

        private void NewAnalysis(AnalysisType anType)
        {
            AnalyzeIssues analyze = new AnalyzeIssues(anType);
            int issueCount = 0;
            if (analyze.HasSearchData)
            {
                issueCount = analyze.GetData();
                if (analyze.GetDataFail)
                {
                    ConsoleUtil.PressAnyKeyToContinue();
                }
            } 
            if (issueCount > 0)
            {                
                analyze.ClassifyStates();                
                analyze.WriteToConsole();
                ConsoleUtil.WriteStdLine("---",StdLine.slResponse,false);

                if (anType != AnalysisType.atIssueSummary)
                {
                    ConsoleUtil.WriteStdLine("PRESS 'Y' to Save to csv file",StdLine.slResponse,false);
                    if (Console.ReadKey(true).Key == ConsoleKey.Y)
                    {
                        analyze.WriteToCSV();
                    }
                }

            }
        }
    }
}