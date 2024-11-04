using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenIPC.Events;
using Prism.Events;
using ReactiveUI;

namespace OpenIPC.ViewModels;

public class Tab1ViewModel : ReactiveObject
{
    private readonly IEventAggregator _eventAggregator;
    public ObservableCollection<string> Frequencies58GHz { get; set; }
    public ICommand RestartWfbCommand { get; private set; }

    private readonly Dictionary<int, string> _58frequencyMapping = new()
    {
        { 36, "5180 MHz [36]" },
        { 40, "5200 MHz [40]" },
        { 44, "5220 MHz [44]" },
        { 48, "5240 MHz [48]" },
        { 52, "5260 MHz [52]" },
        { 56, "5280 MHz [56]" },
        { 60, "5300 MHz [60]" },
        { 64, "5320 MHz [64]" },
        { 100, "5500 MHz [100]" },
        { 104, "5520 MHz [104]" },
        { 108, "5540 MHz [108]" },
        { 112, "5560 MHz [112]" },
        { 116, "5580 MHz [116]" },
        { 120, "5600 MHz [120]" },
        { 124, "5620 MHz [124]" },
        { 128, "5640 MHz [128]" },
        { 132, "5660 MHz [132]" },
        { 136, "5680 MHz [136]" },
        { 140, "5700 MHz [140]" },
        { 144, "5720 MHz [144]" },
        { 149, "5745 MHz [149]" },
        { 153, "5765 MHz [153]" },
        { 157, "5785 MHz [157]" },
        { 161, "5805 MHz [161]" },
        { 165, "5825 MHz [165]" },
        { 169, "5845 MHz [169]" },
        { 173, "5865 MHz [173]" },
        { 177, "5885 MHz [177]" }
    };
    
    public Tab1ViewModel()
    {
        InitializeCollections();
        
        RestartWfbCommand = new RelayCommand(() => RestartWfb());
        
        _eventAggregator = App.EventAggregator;
        
        _eventAggregator.GetEvent<TabMessageEvent>().Subscribe(MessageReceived);
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(AppMessageReceived);
    }

    private void AppMessageReceived(AppMessage obj)
    {
        Console.WriteLine($"******* Tab1ViewModel : AppMessageReceived: {obj}");
    }

    private void RestartWfb()
    {
        _eventAggregator.GetEvent<TabMessageEvent>().Publish("Button Pushed");
    }

    private void MessageReceived(string obj)
    {
        Console.WriteLine($"******* Tab1ViewModel : MessageReceived: {obj}");
    }

    private void InitializeCollections()
    {
        Frequencies58GHz = new ObservableCollection<string>(_58frequencyMapping.Values);
    }
}