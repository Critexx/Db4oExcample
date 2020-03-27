using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Db4objects.Db4o;

namespace Subventionsvergabe
{
	public class Db4o
	{
		// Folder wo die Exe liegt.
		readonly static string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Subventionsvergabe.yap");

		private IObjectContainer db;

		public Db4o()
		{
			db = Db4oFactory.OpenFile(filePath);
		}

		public List<T> LoadData<T>()
		{
			var result = from T o in db.Query<T>(typeof(T))
						 select o;

			return result.ToList();
		}

		internal void UpdateAntrag(Antrag antrag)
		{
			var result = db.QueryByExample(new Antrag() { Id = antrag.Id });
			Antrag antragUpdate = (Antrag) result.Next();
			antragUpdate.Status = antrag.Status;
			antragUpdate.isErstdatenKorrekt = antrag.isErstdatenKorrekt;
			db.Store(antragUpdate);
		}

		public void Persist(params object[] obj)
		{
			db.Store(obj);
		}

		public void FillHausbesitzer()
		{
			List<Hausbesitzer> result = LoadData<Hausbesitzer>();
			if(result.Any()) return;

			List<Hausbesitzer> hausbesitzerList = new List<Hausbesitzer>
			{
				new Hausbesitzer(){
					Id = 1,
					Name = "Peter Schmid",
					Adresse = "Radweg 5",
					PLZ = "8640",
					Ort = "Rapperswil"
				},
				new Hausbesitzer(){
					Id = 2,
					Name = "Corinne Meier",
					Adresse = "Lättenstrasse 15",
					PLZ = "6000",
					Ort = "Luzern"
				},
				new Hausbesitzer(){
					Id = 3,
					Name = "Thomas Weber",
					Adresse = "Sesamstrasse 22",
					PLZ = "6300",
					Ort = "Zug"
				},
				new Hausbesitzer(){
					Id = 4,
					Name = "David Huber",
					Adresse = "Sonnenweg 35",
					PLZ = "4004",
					Ort = "Basel"
				},
				new Hausbesitzer(){
					Id = 5,
					Name = "Céline Schneider",
					Adresse = "Rosenweg 33",
					PLZ = "8000",
					Ort = "Zürich"
				},
			};
			Persist(hausbesitzerList);
		}

		internal void Close()
		{
			db.Close();
		}
	}
}