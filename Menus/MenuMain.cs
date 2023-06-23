using System.Text;
using System.Buffers;
using System;



using Spectre.Console;

namespace JiraCon
{


    public class MenuMain : IMenuConsole
    {
        public JTISConfig ActiveConfig {get;set;}        
        public MenuMain(JTISConfig cfg)
        {
            ActiveConfig = cfg;                        
        }

        public void BuildMenu2()
        {
            var cfgName = string.Format("Connected: {0} ",ActiveConfig.configName);
            ConsoleUtil.WriteAppTitle();
            var menuInfo = string.Format("[bold {0} on {1}]  MAIN MENU [/][grey84 on {1}]| {2} [/]",StdLine.slMenuName.BackMkp(),StdLine.slMenuName.FontMkp(),cfgName);
            var panel = new Panel(menuInfo);
            
            panel.Border = BoxBorder.Rounded;
            panel.BorderColor( ConsoleUtil.StdStyle(StdLine.slMenuName).Foreground);
            panel.HeaderAlignment(Justify.Center );
            panel.Padding = new Padding(2, 0, 2, 0);
            AnsiConsole.Write(panel);

            var detList = new List<string>();
                detList.Add(" 01: Analyze Issue(s) Time In Status");
                detList.Add(" 02: Show Change History for 1 or more Cards");
                detList.Add(" 03: Show JSON for 1 or more Cards");
                detList.Add(" 04: Config Menu");
                detList.Add(" 05: Dev/Misc Menu");
                detList.Add(" [bold dim]EXIT[/]");


            StringBuilder sb = new StringBuilder();
            var mkFont = StdLine.slMenuDetail.FontMkp();
            string menuTitle = string.Empty;
            sb = sb.Append(handler: $"[{StdLine.slMenuDetail.FontMkp()} on {StdLine.slMenuDetail.BackMkp()}]").Append("[italic]USE [bold]ARROW KEYS[/] TO SELECT, THEN PRESS [bold]ENTER[/][/]");
           string resp = AnsiConsole.Prompt<string>(new SelectionPrompt<string>()
                    //.Title(sb.ToString())
                    .AddChoices(detList.ToArray()));
                    // .PageSize(10)
                    // .MoreChoicesText("(Move up and down to reveal more choices)")
                    // .AddChoices(menuRows));
                    

        }

        private void BuildMenu()
        {
            var cfgName = string.Format("Connected: {0} ",ActiveConfig.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();
            lines.AddConsoleLine(" ------------- " + padd, ConsoleUtil.StdStyle(StdLine.slMenuName));
            lines.AddConsoleLine("|  Main Menu  |" + " " + cfgName, ConsoleUtil.StdStyle(StdLine.slMenuName));
            lines.AddConsoleLine(" ------------- " + padd, ConsoleUtil.StdStyle(StdLine.slMenuName));
            lines.AddConsoleLine("(A) Analyze Issue(s) Time In Status", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(M) Show Change History for 1 or more Cards", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(J) Show JSON for 1 or more Cards", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            // lines.AddConsoleLine("(F) Create Extract Files", StdLine.slMenuDetail);
            // lines.AddConsoleLine("(W) Create Work Metrics Analysis from JQL Query", StdLine.slMenuDetail);
            // lines.AddConsoleLine("(E) Epic Analysis - Find and Analyze - Yep, this exists", StdLine.slMenuDetail);
            lines.AddConsoleLine("",ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(C) Config Menu", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(D) Dev/Misc Menu", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("Enter selection or X to exit.", ConsoleUtil.StdStyle(StdLine.slResponse));
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
            ConsoleKeyInfo resp = default(ConsoleKeyInfo);

            if (key == ConsoleKey.M)
            {
                ConsoleUtil.WriteStdLine("Enter 1 or more card keys separated by a space (e.g. POS-123 POS-456 BAM-789), or ENTER to cancel", StdLine.slResponse, false);
                var keys = Console.ReadLine().ToUpper();
                if (string.IsNullOrWhiteSpace(keys))
                {
                    return true;
                }
                string[] arr = keys.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length >= 1)
                {
                    List<JIssue>? retIssues;
                    retIssues = MainClass.AnalyzeIssues(keys);                    
                    if (retIssues != null)
                    {
                        ConsoleUtil.WriteStdLine("Enter 'Y' to save output to csv file, otherwise press any key",StdLine.slResponse,false);
                        resp = Console.ReadKey(true);
                        if (resp.Key == ConsoleKey.Y)
                        {
                            MainClass.WriteChangeLogCSV(retIssues);                                
                            ConsoleUtil.PressAnyKeyToContinue();
                        }
                    }
                }
                return true;
            }
            else if (key == ConsoleKey.F)
            {                
                ConsoleUtil.WriteStdLine("",StdLine.slResponse);
                ConsoleUtil.WriteStdLine("Enter or paste JQL, or ENTER to return",StdLine.slResponse,false);
                var jql = Console.ReadLine();
                if (jql.Length > 0)
                {
                    ConsoleUtil.WriteStdLine(string.Format("Enter (Y) to use the following JQL?\r\n***** {0}", jql),StdLine.slResponse,false);
                    var keys = Console.ReadKey(true);
                    if (keys.Key == ConsoleKey.Y)
                    {
                        ConsoleUtil.WriteStdLine("Enter (Y)es to include card descriptions and comments in the Change History file, otherwise press any key",StdLine.slResponse,false);
                        var k = Console.ReadKey(true);
                        bool includeCommentsAndDesc = false;
                        if (k.Key == ConsoleKey.Y)
                        {
                            includeCommentsAndDesc = true;
                        }
                        MainClass.CreateExtractFiles(jql,includeCommentsAndDesc);
                        ConsoleUtil.PressAnyKeyToContinue();
                    }

                }
                return true;
            }
            else if (key == ConsoleKey.J)
            {
                ConsoleUtil.WriteStdLine("",StdLine.slResponse);
                ConsoleUtil.WriteStdLine("Enter or paste JQL then press enter to continue.",StdLine.slResponse);
                var jql = Console.ReadLine();
                if (jql.Length > 0)
                {
                    ConsoleUtil.WriteStdLine("",StdLine.slResponse);
                    ConsoleUtil.WriteStdLine(string.Format("Enter (Y) to use the following JQL?\r\n***** {0}", jql),StdLine.slResponse);
                    var keys = Console.ReadKey(true);
                    if (keys.Key == ConsoleKey.Y)
                    {
                        MainClass.ShowJSON(jql);
                        ConsoleUtil.PressAnyKeyToContinue();
                    }
                }
                return true;

            }
            else if (key == ConsoleKey.C)
            {
                while (MenuManager.DoMenu(new MenuConfig(ActiveConfig)))
                {

                }
                return true;
            }
            else if (key == ConsoleKey.D)
            {
                while (MenuManager.DoMenu(new MenuDev(ActiveConfig)))
                {

                }
                return true;
            }
            else if (key == ConsoleKey.A)
            {
                while (MenuManager.DoMenu(new MenuIssueStates(ActiveConfig)))
                {

                }
                return true;
            }

            else if (key == ConsoleKey.X)
            {
                return false;
            }
            return true;
        }
    }
}