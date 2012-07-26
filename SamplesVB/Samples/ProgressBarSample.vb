
'ProgressBarSample.vb

'Author:
'      Andres G. Aragoneses <knocte@gmail.com>
'      Peter Gill <peter@majorsilence.com>

'Copyright (c) 2012 Andres G. Aragoneses

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
Imports System.Timers
Imports Xwt

Namespace Samples
    Public Class ProgressBarSample
        Inherits VBox

        Private timer As Timer = New Timer(100.0)

        Private determinateProgressBar As ProgressBar

        Private indeterminateProgressBar As ProgressBar

        Public Sub New()
            Me.indeterminateProgressBar = New ProgressBar()
            MyBase.PackStart(Me.indeterminateProgressBar, BoxMode.FillAndExpand)
            Me.indeterminateProgressBar.Indeterminate = True
            AddHandler Me.timer.Elapsed, AddressOf Me.Increase
            Me.determinateProgressBar = New ProgressBar()
            Me.determinateProgressBar.Fraction = 0.0
            MyBase.PackStart(Me.determinateProgressBar, BoxMode.FillAndExpand)
            Me.timer.Start()
        End Sub

        Public Sub Increase(sender As Object, args As ElapsedEventArgs)
            Dim currentFraction As Double? = New Double?(Me.determinateProgressBar.Fraction)
            Dim nextFraction As Double
            If currentFraction.HasValue AndAlso currentFraction.Value >= 0.0 AndAlso currentFraction.Value <= 0.9 Then
                nextFraction = currentFraction.Value + 0.1
            Else
                nextFraction = 0.0
            End If
            Application.Invoke(Sub()
                                   Me.determinateProgressBar.Fraction = nextFraction
                               End Sub)
        End Sub
    End Class
End Namespace
