﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace restaurant
{
    public partial class IO
    {
        private Database database = new Database();
        public void Savedatabase(Database database)
        {
            if (!FileSystem.DirectoryExists(@"..\database\"))
            {
                FileSystem.CreateDirectory(@"..\database\");
            }

            
            string output = JsonConvert.SerializeObject(database, Formatting.Indented);
            // @ neemt tekst letterlijk, geen \n bijv.
            File.WriteAllText(@"..\database\database.Json", output);
        }

        public Database Getdatabase()
        {
            Database database = new Database();
            
            if (!File.Exists(@"..\database\database.Json")) return database;
            string output = File.ReadAllText(@"..\database\database.Json");
            database = JsonConvert.DeserializeObject<Database>(output);

            List<Tafels> temp = new List<Tafels>();
            for (int i = 0; i < 100; i++)
            {
                Tafels tafel = new Tafels
                {
                    ID = i,
                    Zetels = 4
                };

                if (i % 2 != 0) tafel.isRaam = true;

                temp.Add(tafel);
            }
            database.tafels = temp;

            return database;
        }
        
        //Deze functie is klaar en kan geknipt worden naar het daadwerkelijke IO bestand. Maakt de boel overzichtelijker
        public void Reset_filesystem()
        {
            try
            {
                FileSystem.DeleteDirectory(@"..\database\", DeleteDirectoryOption.DeleteAllContents);
            }
            catch
            {
            }

            if (!FileSystem.DirectoryExists(@"..\database\"))
            {
                FileSystem.CreateDirectory(@"..\database\");
            }
        }

        public List<Tuple<DateTime, List<Tafels>>> Reservering_beschikbaarheid(int dagen)
        {
            //maakt een lijst met tuples die beheert alle beschikbare plekken op int aantal dagen
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = new List<Tuple<DateTime, List<Tafels>>>();

            //vult de List met alle beschikbare momenten en tafels
            DateTime possibleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 0, 0);
            for (int days = 0; days <= dagen; days++)
            {
                //48 kwaterieren van 1000 tot 2130
                for (int i = 0; i < 47; i++)
                {
                    possibleTime = possibleTime.AddMinutes(15);
                    beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                }
                //gaat naar de volgende dag met de openingsuren
                possibleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 10, 0, 0);
            }


            //verantwoordelijk voor het communiceren met de database
            foreach (var reservering in database.reserveringen)
            {
                //voor de datum tussen nu en de ingevoerde dag
                if (reservering.datum.Date == DateTime.Now.Date || reservering.datum.Date <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + dagen, 21, 0, 0))
                {
                    //temptablelist bevat alle tafels
                    List<Tafels> tempTableList = beschikbaar[beschikbaar.IndexOf(Tuple.Create(reservering.datum, database.tafels))].Item2;

                    //gaat door alle gereserveerde tafels in die reservering en haalt deze weg
                    foreach (var tafel in reservering.tafels)
                    {
                        tempTableList.Remove(tafel);
                    }

                    //als er geen tafels meer vrij zijn haalt hij de tafel weg
                    if (tempTableList.Count == 0)
                        for (int a = 0; a < 8; a++)
                        {
                            beschikbaar.Remove(Tuple.Create(new DateTime(reservering.datum.Year, reservering.datum.Month, reservering.datum.Day, a * 15, 0, 0), tempTableList));
                        }

                    //maakt tuple met tafels die wel beschikbaar zijn
                    else
                    {
                        for (int a = 0; a < 8; a++)
                        {
                            beschikbaar[beschikbaar.IndexOf(Tuple.Create(reservering.datum, database.tafels))] = Tuple.Create(new DateTime(reservering.datum.Year, reservering.datum.Month, reservering.datum.Day, a * 15, 0, 0), tempTableList);
                        }
                    }
                }
            }
            return beschikbaar;
        }
    }
}