Imports System
Imports System.Reflection
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class Images
        Inherits VBox

        Public Sub New()
  
            Dim img As New ImageView()
            img.Image = Image.FromResource(MyBase.GetType, "cow.jpg")
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
