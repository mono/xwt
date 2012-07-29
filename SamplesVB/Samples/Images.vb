
'Images.vb

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
Imports System.Reflection
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
	Public Class Images
		Inherits VBox

		Public Sub New()

			Dim img As New ImageView()

			Dim ms As New IO.MemoryStream()
			SamplesVB.My.Resources.cow.Save(ms, System.Drawing.Imaging.ImageFormat.Png)

			img.Image = Image.FromStream(ms)
			PackStart(img)

			Dim stockIcons As FieldInfo() = GetType(StockIcons).GetFields(BindingFlags.[Static] Or BindingFlags.[Public])
			Dim perRow As Integer = 6
			Dim row As HBox = Nothing
			For i As Integer = 0 To stockIcons.Length - 1

				If Not (stockIcons(i).FieldType IsNot GetType(String)) Then
					If i Mod perRow = 0 Then
						If row IsNot Nothing Then
							MyBase.PackStart(row)
						End If
						row = New HBox()
					End If

					Dim vbox As VBox = New VBox()
					Dim stockId As String = CStr(stockIcons(i).GetValue(Nothing))
					Dim imageView As ImageView = New ImageView()
					Dim label As Label = New Label(stockId)

					Try
						Dim icon As Image = Image.FromIcon(stockId, IconSize.Medium)
						If icon IsNot Nothing Then
							imageView.Image = icon
						End If
					Catch ex As Exception
					End Try

					vbox.PackStart(imageView)
					vbox.PackEnd(label)
					row.PackStart(vbox)
				End If
			Next

			If row IsNot Nothing Then
				MyBase.PackStart(row)
			End If
		End Sub
	End Class
End Namespace
