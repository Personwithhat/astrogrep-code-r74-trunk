using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Drawing;

namespace AstroGrep
{
    public class CustomComboBox : ComboBox
    {
        // expose properties as needed
       // public Color SelectedBackColor { get; set; }

        // constructor
        public CustomComboBox()
        {
            DrawItem += new DrawItemEventHandler(DrawCustomMenuItem);
            DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;

       //     SelectedBackColor = Color.LightSeaGreen;
        }

        protected void DrawCustomMenuItem(object sender, DrawItemEventArgs e)
        {
            //ComboBox cBox = sender as ComboBox;
            e.DrawBackground();

            // A dropdownlist may initially have no item selected, so skip the highlighting:
            if (e.Index >= 0)
            {
                Brush text = new SolidBrush(System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(177)))), ((int)(((byte)(10))))));
                Brush back = new SolidBrush(System.Drawing.Color.Black);
                Brush brush = ((e.State & DrawItemState.Selected) > 0) ? text : new SolidBrush(ForeColor);
                //Rectangle rectangle = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.FillRectangle((e.State & DrawItemState.Selected) > 0 ? back : new SolidBrush(BackColor), e.Bounds);
                e.Graphics.DrawString(this.Items[e.Index].ToString(), e.Font, brush, e.Bounds, StringFormat.GenericDefault);

                text.Dispose(); back.Dispose(); brush.Dispose();
            }
            e.DrawFocusRectangle();
        }
    }
}
