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
