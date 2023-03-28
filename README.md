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
    </StackPanel>
```

[![AntDesign](https://img.shields.io/nuget/dt/AntWpf)](https://www.nuget.org/packages/AntWpf/)
[![AntDesign](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](https://github.com/Mumtozbekov/AntWpf/blob/master/AntWpf/LICENSE.txt)


</div>

