# AntWpf
A UI framework using [ant design](https://ant.design/docs/spec/introduce) language to help developers build their own WPF applications.

##### Under development, not suitable for production environment

# GETTING STARTED

``` 
<Application ...>
  <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/AntWpf;component/Styles/Theme.xaml" />
                <ResourceDictionary Source="pack://application:,,,/AntWpf;component/Styles/Controls.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

![Buttons](https://github.com/mumtozbekov/AntWpf/blob/master/AntWpf/images/buttons.png?raw=true)
```
    xmlns:Ant="clr-namespace:AntWpf.Controls;assembly=AntWpf"

    <WrapPanel Orientation="Horizontal" VerticalAlignment="Center" HorizontalAlignment="Center">
        <Ant:Button Type="Primary" Margin="10,0" Content="Button1"/>
        <Ant:Button Type="Primary" Shape="Circle" Icon="search" Margin="10,0" />
        <Ant:Button Type="Dashed" Margin="10,0" Content="Button2"/>
        <Ant:Button Type="Danger" Margin="10,0" Content="Button3"/>
        <Ant:Button Type="Primary" Margin="10,10" Loading="True" Content="Loading"/>
        <Ant:Button Type="Primary" Shape="Circle"  Margin="10,10" Loading="True" />
    </WrapPanel>
```

![Inputs](https://github.com/mumtozbekov/AntWpf/blob/master/AntWpf/images/inputs.png?raw=true)
```
    xmlns:Ant="clr-namespace:AntWpf.Controls;assembly=AntWpf"

    <StackPanel Grid.Row="1" VerticalAlignment="Center" MinWidth="300"  HorizontalAlignment="Center">
      <TextBox Ant:Input.Placeholder="Place holder" Margin="0,10"/>
      <PasswordBox Ant:Input.Placeholder="PassworBox" Ant:Input.Eyeable="True"/>
      <ComboBox Margin="0,10" >
       <ComboBoxItem Content="Itemm 1"/>
       <ComboBoxItem Content="Itemm 3"/>
       <ComboBoxItem Content="Itemm 4"/>
     </ComboBox>
    </StackPanel>
```
![Inputs](https://github.com/mumtozbekov/AntWpf/blob/master/AntWpf/images/check_radios.png?raw=true)
```
  <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center" MinWidth="300"  HorizontalAlignment="Center">
    <CheckBox Margin="5,0" Content="Checked" IsChecked="True"/>
    <CheckBox Margin="5,0" Content="UnChecked"/>
    <CheckBox Margin="5,0" Content="Disabled" IsEnabled="False"/>
</StackPanel>
<StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center" MinWidth="300" Margin="0,12" HorizontalAlignment="Center">
    <Ant:Switch Margin="5,0" UnCheckedContent="0" Content="1" IsChecked="True"/>
    <Ant:Switch Margin="5,0" UnCheckedContent="UnChecked" IsChecked="True" Content="Checked"/>
    <Ant:Switch Margin="5,0" IsEnabled="False"/>
    <Ant:Switch Margin="5,0"  IsChecked="True"  Loading="True"/>
</StackPanel>
<StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center" MinWidth="300" Margin="0,12" HorizontalAlignment="Center">
    <RadioButton Margin="5,0" GroupName="0" Content="Checked" IsChecked="True"/>
    <RadioButton Margin="5,0" GroupName="0" Content="UnChecked" />
    <RadioButton Margin="5,0" GroupName="0" Content="Disabled" IsEnabled="False" />
</StackPanel>
```
![Inputs](https://github.com/mumtozbekov/AntWpf/blob/master/AntWpf/images/icons.png?raw=true)
```
  <Ant:AntIcon Key="{AntCloudOutlined}" />
```
[![AntDesign](https://img.shields.io/nuget/dt/AntWpf)](https://www.nuget.org/packages/AntWpf/)
[![AntDesign](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](https://github.com/Mumtozbekov/AntWpf/blob/master/AntWpf/LICENSE.txt)


</div>

