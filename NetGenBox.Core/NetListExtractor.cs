using System.Collections.Immutable;
using NetGenBox.Core.Xilinx;
using NetGenBox.Parser.Types;

namespace NetGenBox.Core;

public class NetListExtractor
{
    private readonly LispList _topList;
    private readonly List<XilinxComment> _xilinxComments;
    private readonly List<XilinxCell> _cells;
    private readonly List<XilinxInstance> _instances;

    private readonly List<Net> _nets;
    public NetListExtractor(LispList topList)
    {
        _topList = topList;
        _xilinxComments = new();
        _cells = new();
        _instances = new();
        _nets = new();
    }

    public ImmutableList<XilinxCell> XilinxCells => _cells.ToImmutableList();

    public ImmutableList<XilinxInstance> XilinxInstances => _instances.ToImmutableList();


    public ImmutableList<Net> Nets => _nets.ToImmutableList();
    
    //If you were dead or still alive, I don't care



    private void ParseStatus(LispList lispList)
    {
        var writtenList = lispList[1] as LispList;
        if ( writtenList is not null && writtenList.Head is LispAtom {Literal:"written"})
        {
            var comments = writtenList.Expressions[1..]
                .OfType<LispList>()
                .ToImmutableList();
            foreach (var comment in comments)
            {
                string name = comment?.Head?.ToString() ?? "";
                string val = comment?.Expressions[1..].ToString() ?? "";
                _xilinxComments.Add(new XilinxComment()
                {
                    Name = name,
                    Value = val
                });
            }
        }
    }


    public void ParseCells(LispList lispList, string libraryName)
    {
        var cells = lispList.Expressions.OfType<LispList>()
            .Where(t => t.Head is LispAtom { Literal: "cell" })
            .ToImmutableList();
        //TODO Need a little bit more cleanup
        foreach (var cell in cells)
        {
            string cellName = "", renamedCellName = "";

            if (cell[1] is LispAtom cellNameAtom)
            {
                cellName = cellNameAtom.Literal;
            }
            else if (cell[1] is LispList cellNameList)
            {
                cellName = (cellNameList[1] as LispAtom)!.Literal;
                renamedCellName = (cellNameList[2] as LispAtom)!.Literal;
            }
            else
                continue;
            
            //string cellName = (cell[1] as LispAtom)!.ToString() ?? "";
            var xilinxCell = new XilinxCell()
            {
                Name = cellName,
                RenamedFrom = renamedCellName,
                LibraryName = libraryName
            };
            var view = cell.Expressions.OfType<LispList>()
                .FirstOrDefault(t => t.Head is LispAtom { Literal: "view" });
            if (view is not null)
            {
                string viewName = (view.Expressions[1] as LispAtom)!.Literal;
                var xilinxView = new XilinxView()
                {
                    ParentCell = xilinxCell,
                    ViewName = viewName,
                };
                ////
                xilinxCell.View = xilinxView;
                ////
                var @interface = view.Expressions.OfType<LispList>()
                    .FirstOrDefault(t => t.Head is LispAtom { Literal: "interface" });
                if (@interface is not null)
                {
                    var ports = @interface.Expressions.OfType<LispList>()
                        .Where(t => t.Head is LispAtom { Literal: "port" })
                        .ToImmutableList();

                    foreach (var port in ports)
                    {
                        //Check port is single wire
                        if (port[1] is LispAtom atom)
                        {
                            string portName = atom.Literal;
                            var directionList = port[2] as LispList;
                            string direction = (directionList[1] as LispAtom).Literal;
                            var pin = new Pin()
                            {
                                Name = portName,
                                IoType = direction == "INPUT" ? Pin.PinType.Input : Pin.PinType.Output
                            };
                            xilinxView.Interfaces.Add(pin);
                        }
                        //Port is bus array <0:n>
                        else if (port[1] is LispList { Head: LispAtom {Literal: "array"}} arrayList && arrayList[1] is LispList nameList)
                        {
                            string originalName = (nameList[1] as LispAtom)!.Literal;
                            string renamedName = (nameList[2] as LispAtom)!.Literal;
                            bool ok = int.TryParse((arrayList[2] as LispAtom)!.Literal, out int len);
                            if (!ok)
                                continue;
                            var directionList = (port[2] as LispList);
                            string direction = (directionList!.Expressions[1] as LispAtom)!.Literal;
                            var arrayPin = new ArrayPin()
                            {
                                Name = originalName,
                                IoType = direction == "INPUT" ? Pin.PinType.Input : Pin.PinType.Output,
                                RenamedFrom = renamedName,
                                StartIndex = 0,
                                StopIndex = len - 1
                            };
                            xilinxView.Interfaces.Add(arrayPin);
;                        }
                        
                    }
                }
                
                var contents = view.Expressions.OfType<LispList>()
                    .FirstOrDefault(t => t.Head is LispAtom { Literal: "contents" });

                if (contents is not null)
                {
                    ParseContents(contents,xilinxCell);
                }

            }
            _cells.Add(xilinxCell);
        }

    }

    private void ParseContents(LispList contents , XilinxCell parent)
    {
        var instances = contents.Expressions.OfType<LispList>()
            .Where(t => t.Head is LispAtom { Literal: "instance" })
            .ToImmutableList();

        var nets = contents.Expressions.OfType<LispList>()
            .Where(t => t.Head is LispAtom { Literal: "net" })
            .ToImmutableList();

        foreach (var instance in instances)
        {
            var nameDefinitionList = instance.Expressions[1];
            var viewRefList = instance.Expressions[2] as LispList;
            string viewRefName = (viewRefList?.Expressions[1] as LispAtom)!.Literal;
            var cellRefList = viewRefList.Expressions[2] as LispList;
            string cellRefName = (cellRefList?.Expressions[1] as LispAtom)!.Literal;
            var referenceCell = _cells.FirstOrDefault(t => t.Name == cellRefName);
            if (nameDefinitionList is LispAtom atom)
            {
                if (parent.View.ViewName == viewRefName && referenceCell is not null)
                {
                    var xilinxInstance = new XilinxInstance()
                    {
                        InstanceName = atom.Literal,
                        ReferenceCell = referenceCell,
                        ParentCell = parent,
                    };
                    _instances.Add(xilinxInstance);
                }
            }
            else if (nameDefinitionList is LispList { Head: LispAtom{Literal:"rename"}} list )
            {
                string originalName = (list[1] as LispAtom)!.Literal;
                string renamedName = (list[2] as LispAtom)!.Literal;
                if (parent.View.ViewName == viewRefName && referenceCell is not null)
                {
                    var xilinxInstance = new XilinxInstance()
                    {
                        InstanceName = originalName,
                        RenamedName = renamedName,
                        ReferenceCell = referenceCell,
                        ParentCell = parent
                    };
                    _instances.Add(xilinxInstance);
                }
                
            }
        }

        foreach (var net in nets)
        {
            string netName= "", renamedNetName = "";
            if (net[1] is LispAtom atom)
            {
                netName = atom.Literal;
            }
            else if (net[1] is LispList list)
            {
                netName = (list[1] as LispAtom)!.Literal;
                renamedNetName = (list[2] as LispAtom)!.Literal;
            }
            
            var cellNet = new Net()
            {
                NetName = netName,
                RenamedNetName = renamedNetName,
                BelongsToCell = parent,
            };

            if (net[2] is LispList { Head: LispAtom { Literal: "joined" } } joinedList)
            {
                var portRefs = joinedList.Expressions.OfType<LispList>()
                    .Where(t => t.Head is LispAtom { Literal: "portRef" }).ToImmutableList();

                foreach (var portRef in portRefs)
                {
                    //TODO handle member keyword


                    if (portRef[1] is LispAtom portRefAtom)
                    {
                        string portRefName = portRefAtom.Literal;
                        XilinxInstance? instance = null;
                        if (portRef.Expressions.Count == 3 && portRef[2] is LispList instanceRefList)
                        {
                            string instanceName = (instanceRefList![1] as LispAtom)!.Literal;
                            instance = _instances.FirstOrDefault(t =>
                                t.InstanceName == instanceName || t.RenamedName == instanceName);
                        }
                    
                        cellNet.PortReferences.Add(new PortReference()
                        {
                            PortName = portRefName,
                            Instance = instance,
                        });
                    }
                    else if (portRef[1] is LispList { Head: LispAtom {Literal: "member"} } portRefList)
                    {
                        string busName = (portRefList[1] as LispAtom)!.Literal;
                        string idx = (portRefList[2] as LispAtom)!.Literal;
                        bool ok = int.TryParse(idx, out int index);
                        if (ok)
                        {
                            cellNet.PortReferences.Add(new PortReferenceBus()
                            {
                                PortName = busName,
                                BusIndex = index
                            });
                        }
                    }
                    
                  
                    
                }
            }
            
            _nets.Add(cellNet);
            
        }
    }

    private void ParseLibrary(LispList lispList)
    {
        if (lispList.Expressions[1] is LispAtom atom)
        {
            ParseCells(lispList, atom.Literal);
        }
    }
    
    public void Parse()
    {
        if (_topList.Expressions.Count == 0)
            return;
        //string edifName;
        var innerExpression = _topList.Expressions[0] as LispList;
        foreach (var expression in innerExpression.Expressions)
        {
            //Console.WriteLine("Expression: " + (++cnt));
            if (expression is LispList lispList)
            {
                if (lispList.Head is LispAtom { Literal: "status" })
                {
                    ParseStatus(lispList);
                }
                else if (lispList.Head is LispAtom { Literal: "external" })
                {
                    ParseCells(lispList, "external");
                }
                else if (lispList.Head is LispAtom { Literal: "library" })
                {
                    ParseLibrary(lispList);
                }
            }
        }

       
    }
    
    
}