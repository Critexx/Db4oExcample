using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Subventionsvergabe
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Db4o db;

		public MainWindow()
		{
			InitializeComponent();
			db = new Db4o();
			LoadHausbesitzer();
			hideAllOtherButtons();
		}

		private void LoadHausbesitzer()
		{
			List<Hausbesitzer> HausbesitzerList = new List<Hausbesitzer>();
			HausbesitzerList.AddRange(db.LoadData<Hausbesitzer>());

			listViewHausbesitzer.ItemsSource = HausbesitzerList;
			listViewHausbesitzer.Items.Refresh();
			if(listViewHausbesitzer.HasItems)
			{
				listViewHausbesitzer.SelectedIndex = 0;
			}
		}

		private void Button_Click_NeuerAntrag(object sender, RoutedEventArgs e)
		{
			int selectedHausbesitzerId = ((Hausbesitzer) listViewHausbesitzer.SelectedItem).Id;

			Antrag newAntrag = new Antrag { Id = Antrag.GetNextId(db), HausbesitzerId = selectedHausbesitzerId };

			List<Antrag> antraege = db.LoadData<Antrag>().Where(x => x.HausbesitzerId == selectedHausbesitzerId).ToList();
			antraege.Add(newAntrag);

			listViewAntraege.ItemsSource = antraege;
			listViewAntraege.Items.Refresh();
			db.Persist(newAntrag);
		}

		private void ListViewHausbesitzer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Hausbesitzer hausbesitzer = (Hausbesitzer) listViewHausbesitzer.SelectedItem;
			if(hausbesitzer == null)
			{
				btnNeuerAntrag.Visibility = Visibility.Hidden;
				return;
			}

			btnNeuerAntrag.Visibility = Visibility.Visible;
			listViewAntraege.ItemsSource = db.LoadData<Antrag>().Where(x => x.HausbesitzerId == hausbesitzer.Id);
			listViewAntraege.Items.Refresh();
			if(listViewAntraege.HasItems)
			{
				listViewAntraege.SelectedIndex = 0;
			}
		}


		//private void Button_Click_NeuerHausbesitzer(object sender, RoutedEventArgs e)
		//{
		//	//Hausbesitzer hausbesitzerHoechsteId = this.listViewHausbesitzer.Items.OfType<Hausbesitzer>().OrderByDescending(x => x.Id).FirstOrDefault();

		//	//Hausbesitzer neuerHausbesitzer = new Hausbesitzer();
		//	//neuerHausbesitzer.Id = hausbesitzerHoechsteId?.Id + 1 ?? 1;

		//}

		private void Button_Click_TestdatenInit(object sender, RoutedEventArgs e)
		{
			db.FillHausbesitzer();
			LoadHausbesitzer();
		}

		private void Button_Click_ErstdatenLiefern(object sender, RoutedEventArgs e)
		{
			Antrag selectedAntrag = (Antrag) listViewAntraege.SelectedItem;
			Antrag antrag = db.LoadData<Antrag>().Single(x => x.Id == selectedAntrag.Id);
			antrag.ErstdatenLiefern(ckbox_DatenKorrekt.IsChecked.Value);
			db.UpdateAntrag(antrag);

			List<Antrag> antraege = db.LoadData<Antrag>().Where(x => x.HausbesitzerId == selectedAntrag.HausbesitzerId).ToList();
			listViewAntraege.ItemsSource = antraege;
			listViewAntraege.Items.Refresh();
			ListViewAntraege_SelectionChanged(sender, null);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			db.Close();
			base.OnClosing(e);
		}

		private void Button_Click_ErstdatenPruefen(object sender, RoutedEventArgs e)
		{
			Antrag selectedAntrag = (Antrag) listViewAntraege.SelectedItem;
			Antrag antrag = db.LoadData<Antrag>().Single(x => x.Id == selectedAntrag.Id);
			if(antrag.PruefenAntrag())
			{
				btnErstdatenLiefern.Visibility = Visibility.Hidden;
				ckbox_DatenKorrekt.Visibility = Visibility.Hidden;
			};
			db.UpdateAntrag(antrag);

			List<Antrag> antraege = db.LoadData<Antrag>().Where(x => x.HausbesitzerId == selectedAntrag.HausbesitzerId).ToList();
			listViewAntraege.ItemsSource = antraege;
			listViewAntraege.Items.Refresh();
			ListViewAntraege_SelectionChanged(sender, null);
		}

		private void Button_Click_SubventionZuweisen(object sender, RoutedEventArgs e)
		{
			Antrag selectedAntrag = (Antrag) listViewAntraege.SelectedItem;
			Antrag antrag = db.LoadData<Antrag>().Single(x => x.Id == selectedAntrag.Id);
			antrag.ZuweisenSubvention();

			db.UpdateAntrag(antrag);

			List<Antrag> antraege = db.LoadData<Antrag>().Where(x => x.HausbesitzerId == selectedAntrag.HausbesitzerId).ToList();
			listViewAntraege.ItemsSource = antraege;
			listViewAntraege.Items.Refresh();
			ListViewAntraege_SelectionChanged(sender, null);
		}

		private void ListViewAntraege_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Antrag selectedAntrag = (Antrag) listViewAntraege.SelectedItem;
			if(selectedAntrag == null)
			{
				hideAllOtherButtons();
				return;
			}
			if(selectedAntrag.Status == Status.ErstdatenGeliefert)
			{
				hideAllOtherButtons();
				btnErstdatenPruefen.Visibility = Visibility.Visible;
				btnErstdatenLiefern.Visibility = Visibility.Visible;
				ckbox_DatenKorrekt.Visibility = Visibility.Visible;
			}

			if(selectedAntrag.Status == Status.Neu)
			{
				hideAllOtherButtons();
				btnErstdatenLiefern.Visibility = Visibility.Visible;
				ckbox_DatenKorrekt.Visibility = Visibility.Visible;
			}

			if(selectedAntrag.Status == Status.ErstdatenGeprueft)
			{
				hideAllOtherButtons();
				btnDetaildatenLiefern.Visibility = Visibility.Visible;
			}

			if(selectedAntrag.Status == Status.DetaildatenGeliefert)
			{
				hideAllOtherButtons();
				btnSubventionZuweisen.Visibility = Visibility.Visible;
			}

			if(selectedAntrag.Status == Status.SubventionZugewiesen)
			{
				hideAllOtherButtons();
				btnReportdatenLiefern.Visibility = Visibility.Visible;
			}

			if(selectedAntrag.Status == Status.ReportingGeliefert)
			{
				hideAllOtherButtons();
			}
		}

		private void hideAllOtherButtons()
		{
			btnErstdatenLiefern.Visibility = Visibility.Hidden;
			btnErstdatenPruefen.Visibility = Visibility.Hidden;
			btnErstdatenLiefern.Visibility = Visibility.Hidden;
			ckbox_DatenKorrekt.Visibility = Visibility.Hidden;
			btnDetaildatenLiefern.Visibility = Visibility.Hidden;
			btnSubventionZuweisen.Visibility = Visibility.Hidden;
			btnReportdatenLiefern.Visibility = Visibility.Hidden;
		}

		private void Button_Click_DetaildatenLiefern(object sender, RoutedEventArgs e)
		{
			Antrag selectedAntrag = (Antrag) listViewAntraege.SelectedItem;
			Antrag antrag = db.LoadData<Antrag>().Single(x => x.Id == selectedAntrag.Id);
			antrag.DetaildatenLiefern();

			db.UpdateAntrag(antrag);

			List<Antrag> antraege = db.LoadData<Antrag>().Where(x => x.HausbesitzerId == selectedAntrag.HausbesitzerId).ToList();
			listViewAntraege.ItemsSource = antraege;
			listViewAntraege.Items.Refresh();
			ListViewAntraege_SelectionChanged(sender, null);
		}

		private void Button_Click_ReportdatenLiefern(object sender, RoutedEventArgs e)
		{
			Antrag selectedAntrag = (Antrag) listViewAntraege.SelectedItem;
			Antrag antrag = db.LoadData<Antrag>().Single(x => x.Id == selectedAntrag.Id);
			antrag.ReportingErstellenUndLiefern();

			db.UpdateAntrag(antrag);

			List<Antrag> antraege = db.LoadData<Antrag>().Where(x => x.HausbesitzerId == selectedAntrag.HausbesitzerId).ToList();
			listViewAntraege.ItemsSource = antraege;
			listViewAntraege.Items.Refresh();
			ListViewAntraege_SelectionChanged(sender, null);
		}
	}
}
