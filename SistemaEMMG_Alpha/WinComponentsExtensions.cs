using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SistemaEMMG_Alpha
{
    public static class WinComponentsExtensions
    {
        public static void FillWithDBData(this ComboBox cmbBox, List<DBFormasPago> formasDePago)
        {
            cmbBox.Items.Clear();
            cmbBox.SelectedValuePath = "Key";
            cmbBox.DisplayMemberPath = "Value";

            foreach (DBFormasPago formaDePago in formasDePago)
            {
                cmbBox.Items.Add(new KeyValuePair<long, string>(formaDePago.GetID(), formaDePago.GetName()));
            }

            cmbBox.SelectedIndex = 0;
        }

        public static void FillWithDBData(this ComboBox cmbBox, List<DBMoneda> monedas)
        {
            cmbBox.Items.Clear();
            cmbBox.SelectedValuePath = "Key";
            cmbBox.DisplayMemberPath = "Value";

            foreach (DBMoneda moneda in monedas)
            {
                cmbBox.Items.Add(new KeyValuePair<long, string>(moneda.GetID(), moneda.GetName()));
            }

            cmbBox.SelectedIndex = 0;
        }
        
        public static DBFormasPago SelectedItemAsFormaPago(this ComboBox cmbBox)
        {
            return (cmbBox.SelectedItem is null) ? null : DBFormasPago.GetByID(((KeyValuePair<long, string>)cmbBox.SelectedItem).Key);
        }

        public static DBMoneda SelectedItemAsMoneda(this ComboBox cmbBox)
        {
            return (cmbBox.SelectedItem is null) ? null : DBMoneda.GetByID(((KeyValuePair<long, string>)cmbBox.SelectedItem).Key);
        }

        public static DBTipoEntidad SelectedItemAsTipoEntidad(this ComboBox cmbBox)
        {
            return (cmbBox.SelectedItem is null) ? null : DBTipoEntidad.GetByID(((KeyValuePair<long, string>)cmbBox.SelectedItem).Key);
        }
        public static DBTiposComprobantes SelectedItemAsTipoComprobante(this ComboBox cmbBox)
        {
            return (cmbBox.SelectedItem is null) ? null : DBTiposComprobantes.GetByID(((KeyValuePair<long, string>)cmbBox.SelectedItem).Key);
        }
    }
}
