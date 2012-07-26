'ButtonSample.vb

'Author:
'      Lluis Sanchez <lluis@xamarin.com>
'      Peter Gill <peter@majorsilence.com>

'Copyright (c) 2011 Xamarin Inc

'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:

'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.

'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.

Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class ButtonSample
        Inherits VBox

        Public Sub New()

                Dim b1 As Button = New Button("Click me")
                AddHandler b1.Clicked, Sub()
                                           b1.Label = "Clicked!"
                                       End Sub
                MyBase.PackStart(b1)
                Dim b2 As Button = New Button("Click me")
                b2.Style = ButtonStyle.Flat
                AddHandler b2.Clicked, Sub()
                                           b2.Label = "Clicked!"
                                       End Sub
                MyBase.PackStart(b2)
                MyBase.PackStart(New Button(Image.FromIcon("ZoomIn", IconSize.Medium)))
                Dim mb As MenuButton = New MenuButton("This is a Menu Button")
                Dim men As Menu = New Menu()
                men.Items.Add(New MenuItem("First"))
                men.Items.Add(New MenuItem("Second"))
                men.Items.Add(New MenuItem("Third"))
                men.Items.Add(New SeparatorMenuItem())
                men.Items.Add(New CheckBoxMenuItem("Check") With {.Checked = True})
                men.Items.Add(New RadioButtonMenuItem("Radio") With {.Checked = True})

                Dim ms As New IO.MemoryStream()
                SamplesVB.My.Resources._class.Save(ms, System.Drawing.Imaging.ImageFormat.Png)

                men.Items.Add(New MenuItem("With image") With {.Image = Image.FromStream(ms)})
                mb.Menu = men
                MyBase.PackStart(mb)
                For Each mi As MenuItem In men.Items
                    Dim cmi As MenuItem = mi
                    AddHandler mi.Clicked, Sub()
                                               mb.Label = cmi.Label + " Clicked"
                                           End Sub
                Next
                Dim tb As ToggleButton = New ToggleButton("Toggle me")
                MyBase.PackStart(tb)
                MyBase.PackStart(New Button("Mini button") With {.Style = ButtonStyle.Borderless})
                MyBase.PackStart(New ToggleButton("Mini toggle") With {.Style = ButtonStyle.Borderless})

        End Sub
    End Class
End Namespace
