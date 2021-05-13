using System;
using System.Drawing;
using System.Windows.Forms;

namespace AstroGrep.Windows.Forms
{
   /// <summary>
   /// Base form for all forms to inherit from.
   /// </summary>
   public class BaseForm: Form
   {
      /// <summary>
      /// Load event for all forms
      /// </summary>
      /// <param name="e">system parameter</param>
      protected override void OnLoad(EventArgs e)
      {
         //// aid font scaling and all controls use system font
         foreach (Control ctr in this.Controls)
         {
            //   ctr.Font = SystemFonts.MessageBoxFont;
            Color mainBack = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(15)))), ((int)(((byte)(6)))));
            Color mainT    = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));

            ctr.ForeColor = mainT;
            ctr.BackColor = mainBack;

            // controls in groupboxes are not child of main form
            if (ctr.HasChildren)
            {
               foreach (Control childControl in ctr.Controls)
               {
                 //childControl.Font = SystemFonts.MessageBoxFont;
                 childControl.ForeColor = mainT;
                 childControl.BackColor = mainBack;
               }
            }
         }

         //// aid font scaling
         //AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font; //Font will handle both DPI changes and changes to the system font size setting
         //AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F); //96dpi

         base.OnLoad(e);
      }
   }
}
