using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subventionsvergabe
{
	public class Antrag
	{
		private static int? highestId;
		public int Id { get; set; }
		public Status Status { get; set; }
		public int HausbesitzerId { get; set; }

		public bool isErstdatenKorrekt;

		public Antrag()
		{
			Status = Status.Neu;
		}

		public void ErstdatenLiefern(bool boolDatenKorrekt)
		{
			if(this.Status <= Status.ErstdatenGeliefert)
			{
				isErstdatenKorrekt = boolDatenKorrekt;
				this.Status = Status.ErstdatenGeliefert;
			} else
			{
				throw new InvalidOperationException($"Erstdaten dürfen mit dem Status {this.Status} nicht mehr geliefert werden.");
			}
		}

		public bool PruefenAntrag()
		{
			if(isErstdatenKorrekt)
			{
				this.Status = Status.ErstdatenGeprueft;
				return true;
			} else
			{
				this.Status = Status.ErstdatenGeliefert;
				return false;
			}
		}

		public void DetaildatenLiefern()
		{
			this.Status = Status.DetaildatenGeliefert;
		}

		public void ZuweisenSubvention()
		{
			this.Status = Status.SubventionZugewiesen;
		}

		public void ReportingErstellenUndLiefern()
		{
			this.Status = Status.ReportingGeliefert;
		}

		public static int GetNextId(Db4o db)
		{
			if(Antrag.highestId != null)
			{
				Antrag.highestId++;
			} else
			{
				List<Antrag> antraege = db.LoadData<Antrag>();
				int Id = antraege.OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? 0;
				Antrag.highestId = ++Id;
				
			}
			return Antrag.highestId.Value;
		}
	}

	public enum Status
	{
		Neu,
		ErstdatenGeliefert,
		ErstdatenGeprueft,
		DetaildatenGeliefert,
		SubventionZugewiesen,
		ReportingGeliefert
	}
}
