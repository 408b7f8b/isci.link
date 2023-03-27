using System;
using System.Linq;
using isci.Allgemein;
using isci.Beschreibung;
using isci.Daten;

namespace isci.link
{
    class Program
    {
        static void Main(string[] args)
        {
            var konfiguration = new Parameter("konfiguration.json");

            var beschreibung = new Modul(konfiguration.Identifikation, "isci.link");
            beschreibung.Name = "Link Ressource " + konfiguration.Identifikation;
            beschreibung.Beschreibung = "Modul zur Verknüpfung von Dateneinträgen";
            beschreibung.Speichern(konfiguration.OrdnerBeschreibungen + "/" + konfiguration.Identifikation + ".json");
            
            var struktur = new Datenstruktur(konfiguration.OrdnerDatenstruktur);

            var dm = new Datenmodell(konfiguration.Identifikation);

            System.IO.File.WriteAllText(konfiguration.OrdnerDatenmodelle + "/" + konfiguration.Identifikation + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(dm));

            struktur.DatenmodellEinhängen(dm);
            struktur.DatenmodelleEinhängenAusOrdner(konfiguration.OrdnerDatenmodelle, konfiguration.Identifikation);

            struktur.VerweiseErzeugen();

            struktur.Start();

            var Zustand = new dtInt32(0, "Zustand", konfiguration.OrdnerDatenstruktur + "/Zustand");
            Zustand.Start();

            while(true)
            {
                Zustand.Lesen();
                var erfüllteTransitionen = konfiguration.Aktivzustände.Where(a => a.Eingangszustand == (System.Int32)Zustand.value);
                if (erfüllteTransitionen.Count<Aktivzustand>() > 0)
                {
                    struktur.Lesen();

                    foreach (var eintrag in struktur.verweiseAktiv)
                    {
                        if (eintrag.Key.aenderung)
                        {
                            foreach (var untereintrag in eintrag.Value)
                            {
                                Console.WriteLine(System.DateTime.Now.ToString("O") + ": " + eintrag.Key.Identifikation + " --> " + untereintrag.Identifikation);
                                untereintrag.value = eintrag.Key.value;
                                eintrag.Key.aenderung = false;
                                untereintrag.Schreiben();
                            }
                        }
                    }
                    
                    Zustand.value = erfüllteTransitionen.First<Aktivzustand>().Ausgangszustand;
                    Zustand.Schreiben();
                }
            }
        }
    }
}