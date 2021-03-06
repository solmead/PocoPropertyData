﻿
Imports System.Reflection
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Collections.Specialized
Imports System.Runtime.InteropServices
Imports System.Linq.Expressions
Imports System.Web
Imports PocoPropertyData

Public Module Extensions

    <Extension>
    Public Function ToItem(Of TItem As {Class})(row As DataRow, table As DataTable, Optional getNewObject As Func(Of TItem) = Nothing, Optional columnMapping As Dictionary(Of String, String) = Nothing, optional afterLoad As Action(Of TItem, DataTable, DataRow, List(of string)) = nothing, optional mappings as Dictionary(Of string, string) = nothing) As TItem

        If getNewObject Is Nothing Then
            getNewObject = Function()
                Dim tp As Type = GetType(TItem)
                Dim newItem As TItem = CType(tp.Assembly.CreateInstance(tp.FullName), TItem)
                Return newItem
                           End Function
        End If
        If (mappings is nothing) then
            mappings = GetDefinedMappings(getNewObject())
        end if
        If columnMapping Is Nothing Then
            columnMapping = New Dictionary(Of String, String)()
        End If

        For Each key In columnMapping.Keys
            If (Not mappings.ContainsKey(key)) Then
                mappings.Add(key, columnMapping(key))
            Else
                mappings(key) = columnMapping(key)
            End If
        Next

        Dim colNames = (from c as DataColumn in table.Columns select c.ColumnName).ToList()

        Dim item = getNewObject()
        For col = 0 To table.Columns.Count - 1
            Try
                Dim origColumnName = table.Columns(col).ColumnName.Trim()

                If Not row.IsNull(col) Then
                    Dim column = row(col)
                    dim newColumnName = origColumnName
                    If (mappings.ContainsKey(origColumnName)) Then
                        newColumnName = mappings(origColumnName)
                    End If
                    If (item.DoesPropertyExist(newColumnName, mappings)) then
                        colNames.Remove(origColumnName)
                        item.SetPropertyOn(newColumnName, column, mappings)
                    end if
                Else
                    dim newColumnName = origColumnName
                    If (mappings.ContainsKey(origColumnName)) Then
                        newColumnName = mappings(origColumnName)
                    End If
                    If (item.DoesPropertyExist(newColumnName, mappings)) then
                        colNames.Remove(origColumnName)
                        item.SetPropertyOn(newColumnName, Nothing, mappings)
                    end if
                End If

            Catch ex As Exception
                Dim i = 0
            End Try
        Next

        If (afterLoad IsNot nothing) Then
            afterLoad(item, table, row, colNames)
        End If


        Return item
    End Function
    

    <Extension>
    Public Function ToList(Of TItem As {Class})(table As DataTable, optional getNewObject As Func(Of TItem) = Nothing, Optional columnMapping As Dictionary(Of String, String) = Nothing, optional afterLoad As Action(Of TItem, DataTable, DataRow, List(of string)) = nothing) As List(Of TItem)
        If getNewObject Is Nothing Then
            getNewObject = Function()
                               Dim tp As Type = GetType(TItem)
                               Dim newItem As TItem = CType(tp.Assembly.CreateInstance(tp.FullName), TItem)
                               Return newItem
                           End Function
        End If

        Dim mappings = GetDefinedMappings(getNewObject())
        If columnMapping Is Nothing Then
            columnMapping = New Dictionary(Of String, String)()
        End If

        For Each key In columnMapping.Keys
            If (Not mappings.ContainsKey(key)) Then
                mappings.Add(key, columnMapping(key))
            Else
                mappings(key) = columnMapping(key)
            End If
        Next

        Dim itemList = New List(Of TItem)()
        For Each row As DataRow In table.Rows
            Dim item = row.ToItem(table, getNewObject, nothing, afterLoad, mappings)
            itemList.Add(item)
        Next
        Return itemList


    End Function




    <Extension>
    public Sub SetPropertyOn(Of TItem As {Class})(item As TItem, name As String, value As Object, optional mappings as Dictionary(Of string, string) = nothing)
        Dim pName = name
        If (mappings is nothing) then
            mappings = GetDefinedMappings(item)
        end if
        If (mappings.ContainsKey(name)) Then
            pName = mappings(name)
        End If


        If (pName.Contains(".")) Then
            Dim tstr = Split(pName, ".")
            Dim tp As Type = item.GetPropertyType(tstr(0), mappings)
            Dim pitem = item.GetValue(tstr(0), mappings)
            If pitem Is Nothing Then
                pitem = tp.Assembly.CreateInstance(tp.FullName)
                item.SetValue(tstr(0), pitem)
            End If

            If TypeOf pitem Is IEnumerable Then

            End If
            Try
                SetPropertyOn(Of Object)(pitem, name.Replace(tstr(0) & ".", ""), value, nothing)
            Catch ex As Exception
                Dim i = 0
            End Try
        Else
            item.SetPropOnObj(name, value, mappings)
        End If
    End Sub
    Public Function IsNullableType(ByVal myType As Type) As Boolean
        Return (myType.IsGenericType) AndAlso (myType.GetGenericTypeDefinition() Is GetType(Nullable(Of )))
    End Function

    <Extension>
    private Sub SetPropOnObj(Of TItem As {Class})(item As TItem, headCol As String, column As Object, optional mappings as Dictionary(Of string, string) = nothing)



        If (item.DoesPropertyExist(headCol, mappings)) Then
            Dim tpe = item.GetPropertyType(headCol, mappings)
            Try
                If column Is Nothing AndAlso IsNullableType(tpe) Then
                    'Dim v = Convert.ChangeType(column, tpe)
                    item.SetValue(headCol, column, mappings)
                    Return
                End If
            Catch ex As Exception

            End Try
            Dim isNullable = IsNullableType(tpe)
            Dim bTpe = tpe
            If isNullable Then
                tpe = Nullable.GetUnderlyingType(tpe)
            End If

            Dim IsEnum = tpe.IsEnum

            Dim tName = tpe.FullName.ToUpper()
            If (tName.Contains("DATETIME")) Then
                Dim v As Date
                If column Is Nothing Then
                    column = ""
                End If

                DateTime.TryParse(column.ToString(), v)
                item.SetValue(headCol, v, mappings)
            ElseIf (tName.Contains("BOOL")) Then
                If column Is Nothing Then
                    column = ""
                End If
                column = column.ToString().ToUpper().Replace("YES", "TRUE").Replace("NO", "FALSE").Replace("0", "FALSE").Replace("N", "FALSE").Replace("Y", "TRUE").Replace("1", "TRUE")
                Dim v As Boolean
                Boolean.TryParse(column, v)
                item.SetValue(headCol, v, mappings)
            ElseIf (tName.Contains("INT")) Then
                If column Is Nothing Then
                    column = ""
                End If
                column = column.ToString().Replace("$", "").Replace(",", "")
                Dim v As Double
                Double.TryParse(column, v)
                item.SetValue(headCol, CInt(v), mappings)
            ElseIf (tName.Contains("FLOAT")) Then
                If column Is Nothing Then
                    column = ""
                End If
                column = column.ToString().Replace("$", "").Replace(",", "")
                Dim v As Single
                Single.TryParse(column, v)
                item.SetValue(headCol, v, mappings)
            ElseIf (tName.Contains("DOUBLE")) Then
                If column Is Nothing Then
                    column = ""
                End If
                column = column.ToString().Replace("$", "").Replace(",", "")
                Dim v As Double
                Double.TryParse(column, v)
                item.SetValue(headCol, v, mappings)
            ElseIf (tName.Contains("LONG")) Then
                If column Is Nothing Then
                    column = ""
                End If
                column = column.ToString().Replace("$", "").Replace(",", "")
                Dim v As Double
                Double.TryParse(column, CLng(v))
                item.SetValue(headCol, v, mappings)
            ElseIf (tName.Contains("DECIMAL")) Then
                If column Is Nothing Then
                    column = ""
                End If
                column = column.ToString().Replace("$", "").Replace(",", "")
                Dim v As Decimal
                Decimal.TryParse(column, v)
                item.SetValue(headCol, v, mappings)
            Else
                If Not IsEnum Then
                    Dim v = Convert.ChangeType(column, tpe)
                    item.SetValue(headCol, v)
                Else
                    If column Is Nothing Then
                        column = ""
                    End If
                    Try
                        Dim exists = (From t In [Enum].GetNames(tpe) Where t.ToUpper() = column.ToString().Trim().ToUpper() Select t).Any()
                        If (exists) Then
                            Dim v = [Enum].Parse(tpe, column.ToString())
                            item.SetValue(headCol, v, mappings)
                        Else
                            Dim num = CInt(Val(column.ToString()))

                            Dim numexists = (From t In [Enum].GetValues(tpe) Where t = num Select t).Any()
                            If (numexists) Then
                                item.SetValue(headCol, num, mappings)
                            End If
                        End If
                    Catch ex As Exception
                        Dim i = 0
                    End Try
                End If
            End If



        ElseIf (headCol <> "") Then

            'Throw New Exception("Column Not Handled: [" + headCol + "]")
        End If

    End Sub


    <Extension>
    Public Function Clone(Of TItem As {Class})(item As TItem) As TItem
        Dim tp As Type = item.GetType()
        Dim newItem As TItem = CType(tp.Assembly.CreateInstance(tp.FullName), TItem)
        item.CopyInto(newItem)
        Return newItem
    End Function
    <Extension>
    Public Sub CloneFrom(Of TItem As {Class})(item As TItem, ByVal oldItem As TItem)
        oldItem.CopyInto(item)
    End Sub
    <Extension>
    Public Sub CopyInto(Of TItem As {Class})(item As TItem, ByVal newItem As TItem)
        For Each p In item.GetPropertyNames(onlyBaseTypes:=True, onlyWritable:=True)
            newItem.SetValue(p, item.GetValue(p))
        Next
    End Sub
    <Extension>
    Public Function PropertiesAsCollection(Of TItem As {Class})(item As TItem) As NameValueCollection
        Dim f As New NameValueCollection
        For Each p In item.GetPropertyNames(onlyBaseTypes:=True, onlyWritable:=False)
            Dim v = item.GetValue(p)
            If (v IsNot Nothing) Then
                v = v.ToString()
            End If
            f(p) = v
        Next
        Return f
    End Function
    <Extension>
    Public Function PropertiesAsDictionary(Of TItem As {Class})(item As TItem) As Dictionary(Of String, String)
        Dim f As New Dictionary(Of String, String)
        For Each p In item.GetPropertyNames(onlyBaseTypes:=True, onlyWritable:=False)
            Dim v = item.GetValue(p)
            If (v IsNot Nothing) Then
                v = v.ToString()
            End If
            f.Add(p, v)
        Next
        Return f
    End Function

    <Extension>
    public Function GetProperty(Of TItem As {Class})(item As TItem, propertyName As String, optional mappings as Dictionary(Of string, string) = nothing) As PropertyInfo
        Dim pName = propertyName
        If (mappings is nothing) then
            mappings = GetDefinedMappings(item)
        end if
        If (mappings.ContainsKey(propertyName)) Then
            pName = mappings(propertyName)
        End If

        Dim tp As Type = item.GetType
        Dim prop = tp.GetProperty(pName)
        Return prop
    End Function

    <Extension>
    Public Function GetPropertyType(Of TItem As {Class})(item As TItem, propertyName As String, optional mappings as Dictionary(Of string, string) = nothing) As Type
        Dim prop = item.GetProperty(propertyName, mappings)

        If (prop IsNot Nothing) Then
            Return prop.PropertyType
        End If
        Return GetType(String)
    End Function
    <Extension>
    private Sub SetValue(Of TItem As {Class})(item As TItem, propertyName As String, value As Object, optional mappings as Dictionary(Of string, string) = nothing)
        Dim prop = item.GetProperty(propertyName, mappings)
        If (prop IsNot Nothing AndAlso prop.CanWrite) Then
            prop.SetValue(item, value, Nothing)
        End If
    End Sub

    <Extension>
    Public Function GetValue(Of TItem As {Class})(item As TItem, propertyName As String, optional mappings as Dictionary(Of string, string) = nothing) As Object
        Dim prop = item.GetProperty(propertyName, mappings)
        Dim retVal As Object = Nothing
        If (prop IsNot Nothing AndAlso prop.CanRead) Then
            retVal = prop.GetValue(item, Nothing)
        End If
        Return retVal
    End Function
    <Extension>
    Public Function DoesPropertyExist(Of TItem As {Class})(item As TItem, propertyName As String, optional mappings as Dictionary(Of string, string) = nothing) As Boolean
        Dim pName = propertyName
        If (mappings is nothing) then
            mappings = GetDefinedMappings(item)
        end if
        If (mappings.ContainsKey(propertyName)) Then
            pName = mappings(propertyName)
        End If


        If (pName.Contains(".")) Then
            Dim tstr = Split(pName, ".")
            Dim prop = item.GetProperty(tstr(0), mappings)
            If (prop isnot nothing) then
                Dim tp As Type = item.GetPropertyType(tstr(0), mappings)
                Dim pitem = item.GetValue(tstr(0), mappings)
                If pitem Is Nothing Then
                    pitem = tp.Assembly.CreateInstance(tp.FullName)
                End If
                Return DoesPropertyExist(pitem, propertyName.Replace(tstr(0) & ".", ""))
            end if
            Return false
        Else 
            Dim prop = item.GetProperty(propertyName, mappings)
            Return (prop IsNot Nothing)
        End if
    End Function
    <Extension>
    Public Function GetPropertyNames(Of TItem As {Class})(item As TItem, Optional onlyWritable As Boolean = True, Optional onlyBaseTypes As Boolean = False) As List(Of String)
        Dim tp As Type = item.GetType
        Dim props = tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy)).ToList

        If onlyWritable Then
            props = (From p In props Where p.CanWrite Select p).ToList
        End If
        If onlyBaseTypes Then
            Try
                props = (From p In props
                         Where Not (p.PropertyType.FullName.Contains("Record") OrElse
                         p.PropertyType.FullName.Contains("Set") OrElse
                         p.PropertyType.FullName.Contains("EntitySet") OrElse
                         (p.PropertyType.BaseType IsNot Nothing AndAlso
                          (p.PropertyType.BaseType.FullName.Contains("Record") OrElse
                         p.PropertyType.BaseType.FullName.Contains("Set") OrElse
                         p.PropertyType.BaseType.FullName.Contains("EntitySet"))))).ToList
            Catch ex As Exception

            End Try
        End If

        Return (From p In props Select p.Name).ToList
    End Function


    <Extension>
    public Function GetPropertyAlternateName(Of TItem As {Class})(item As TItem, propertyName As String, optional mappings as Dictionary(Of string, string) = nothing) As string
        dim itm = item.GetProperty(propertyName, mappings)
        Dim attrs As IEnumerable(Of Object) = (From ca In itm.GetCustomAttributes(True) Where ca.TypeId.FullName.Contains("ColumnAttribute") Select ca).ToList()
        'Dim attr As ColumnAttribute = itm.GetCustomAttributes(GetType(ColumnAttribute), True).SingleOrDefault()
        

        Dim attr2 As DisplayAttribute = itm.GetCustomAttributes(GetType(DisplayAttribute), True).FirstOrDefault()
        If (attr2 IsNot Nothing) Then
            Return attr2.Name
        End If

        Dim attr3 As DisplayNameAttribute = itm.GetCustomAttributes(GetType(DisplayNameAttribute), True).FirstOrDefault()
        If (attr3 IsNot Nothing)  Then
            Return attr3.DisplayName
        End If
        Dim attr4 As DescriptionAttribute = itm.GetCustomAttributes(GetType(DescriptionAttribute), True).FirstOrDefault()
        If (attr4 IsNot Nothing)  Then
            Return attr4.Description
        End If

        For Each attr In attrs
            Try
                If (attr IsNot Nothing) AndAlso Not String.IsNullOrWhiteSpace(attr.Name) Then
                    Return attr.Name
                End If
            Catch ex As Exception

            End Try
        Next
        Return  ""
    End Function

    Private Function GetDefinedMappings(Of TItem As {Class})(item As TItem, Optional onlyWritable As Boolean = True, Optional onlyBaseTypes As Boolean = False) As Dictionary(Of String, String)
        Dim tp As Type = item.GetType()

        Dim name = tp.ToString()

        Dim d as Dictionary(Of String, String) = nothing
        Try
            d = CType(HttpRuntime.Cache(name.ToUpper()), Dictionary(Of String, String))
        Catch ex As Exception

        End Try

        If (d Is nothing) then

            Dim props = tp.GetProperties((BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.FlattenHierarchy)).ToList()

            If onlyWritable Then
                props = (From p In props Where p.CanWrite Select p).ToList
            End If
            If onlyBaseTypes Then
                Try
                    props = (From p In props
                             Where Not (p.PropertyType.FullName.Contains("Record") OrElse
                             p.PropertyType.FullName.Contains("Set") OrElse
                             p.PropertyType.FullName.Contains("EntitySet") OrElse
                             (p.PropertyType.BaseType IsNot Nothing AndAlso
                              (p.PropertyType.BaseType.FullName.Contains("Record") OrElse
                             p.PropertyType.BaseType.FullName.Contains("Set") OrElse
                             p.PropertyType.BaseType.FullName.Contains("EntitySet"))))).ToList
                Catch ex As Exception

                End Try
            End If
            'Dim lst2 = (From p In props
            '        From ca In p.GetCustomAttributes(True)
            '        Select new with { 
            '                       .p =p, 
            '                       .ca =ca 
            '                   }).ToList()

            Dim lst = (From p In props
                       Where (From ca In p.GetCustomAttributes(True)
                              Where ca.TypeId.ToString().Contains("ColumnAttribute")
                              Select ca).Any() OrElse
                        p.GetCustomAttributes(GetType(DisplayAttribute), True).Any() OrElse
                        p.GetCustomAttributes(GetType(DisplayNameAttribute), True).Any()OrElse
                             p.GetCustomAttributes(GetType(DescriptionAttribute), True).Any()
                       Select p).ToList()

            'Dim lst = (From p In props
            '           Where p.GetCustomAttributes(GetType(ColumnAttribute), True).Any()
            '           Select p)


            Dim dic As New Dictionary(Of String, String)
            For Each itm In lst
                Dim attrs As IEnumerable(Of Object) = (From ca In itm.GetCustomAttributes(True) Where ca.TypeId.ToString().Contains("ColumnAttribute") Select ca).ToList()
                'Dim attr As ColumnAttribute = itm.GetCustomAttributes(GetType(ColumnAttribute), True).SingleOrDefault()
                For Each attr In attrs
                    Try
                        If (attr IsNot Nothing) AndAlso Not String.IsNullOrWhiteSpace(attr.Name) Then
                            dic.Add(attr.Name, itm.Name)
                        End If
                    Catch ex As Exception

                    End Try
                Next

                Dim attr2 As DisplayAttribute = itm.GetCustomAttributes(GetType(DisplayAttribute), True).FirstOrDefault()
                If (attr2 IsNot Nothing) AndAlso Not dic.ContainsKey(attr2.Name) Then
                    dic.Add(attr2.Name, itm.Name)
                End If

                Dim attr3 As DisplayNameAttribute = itm.GetCustomAttributes(GetType(DisplayNameAttribute), True).FirstOrDefault()
                If (attr3 IsNot Nothing) AndAlso Not dic.ContainsKey(attr3.DisplayName) Then
                    dic.Add(attr3.DisplayName, itm.Name)
                End If
                Dim attr4 As DescriptionAttribute = itm.GetCustomAttributes(GetType(DescriptionAttribute), True).FirstOrDefault()
                If (attr4 IsNot Nothing)  Then
                    dic.Add(attr4.Description, itm.Name)
                End If

            Next
            d = dic

        End If
        HttpRuntime.Cache(name.ToUpper()) = d
        

        Return d
    End Function



    

End Module
