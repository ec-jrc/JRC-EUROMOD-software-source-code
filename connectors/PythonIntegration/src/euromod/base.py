__license__='''
Copyright 2024 European Commission
*
Licensed under the EUPL, Version 1.2;
You may not use this work except in compliance with the Licence.
You may obtain a copy of the Licence at:

*
   https://joinup.ec.europa.eu/software/page/eupl
*

Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the Licence for the specific language governing permissions and limitations under the Licence.
'''

import os
import clr 
from utils._paths import  DLL_PATH
clr.AddReference(os.path.join(DLL_PATH, "EM_XmlHandler.dll" ))
from EM_XmlHandler import  XmlHelpers, ReadModelOptions
from container import Container

class Base_Element:
    
    def _short_repr(self):
        return f"{self.name}" 
    def _container_repr(self):
        return f"{self.name}"
    def _container_middle_repr(self):
        ### Potential middle_representation of a string
        return ""
    def _container_begin_repr(self):
        return f"{self.name}"
    def _container_end_repr(self):
        return  ""

class Euromod_Element(Base_Element):
    _objectType = None
    _CDATAvars = {'Value','Comment'}
    _extensionType = None
    def __init__(self,info,parent):
        self._info = info #csharp info
        self.parent = parent
    def _get_extension_repr(self):
        ext = ""
        if len(self.extensions) > 0:
            ext = " (with switch set for "
            for i,el in enumerate(self.extensions):
                ext += el.shortName
                if i<len(self.extensions) - 1:
                    ext += ", "
                    
            ext += ") "
        return ext


    
    def __setattr__(self, name, value):
        tempname=name
        if name != "ID": #Check if name exists in csharp dictionary...
            tempname = name[0].upper() + name[1:]
        if "_info" in self.__dict__ and self._info.ContainsKey(tempname):
            valueAdjusted = value
            if tempname in __class__._CDATAvars:
                valueAdjusted = XmlHelpers.CDATA(value)

            self._info[tempname] = valueAdjusted
        elif name == "_info":
            for el in value:
                key = el.Key
                
                valueDict = el.Value
                if str(key) in __class__._CDATAvars:
                    valueDict = XmlHelpers.RemoveCData(valueDict)
                key = get_cleaned_key(key)
                if key in ['bestMatchDatasets','datasets']:
                    super().__setattr__( key, valueDict.split(' '))
                else: 
                    super().__setattr__( key, valueDict) 
                
            
        super().__setattr__(name,value)
          
           
    def get_properties(self):
        properties = [x  for x in super().__dir__() if not x.startswith("_") and x not in  ["get_properties","load_data","run", "model","parent", "parentSystem", "parentTypeObject","show_attr","evaluate"]] 
        properties = [x for x in properties if not x.startswith("get_") ]
        properties = [x for x in properties if not callable(getattr(self,x))]
        properties.sort()
        return properties
    def __repr__(self):
        rep = "-"*30 + "\n"
        rep += self.__class__.__name__ + "\n"
        rep += "-"*30 + "\n"
        for el in self.get_properties():
            attr = getattr(self,el)
            if hasattr(attr,"_short_repr"):
                attr_repr = attr._short_repr()
            else:
                attr_repr = ""
                attr_repr = repr(attr)
            rep += f"\t {el}: {attr_repr}\n"
        return rep
        
    def _linkToExtensions(self):
        self.extensions = Container()
        if self.__class__._extensionType is None:
            return
        parent = self.parent
        while (parent.__class__.__name__ != "Country"):
            parent = parent.parent
        ctry = parent
        for el in (ctry.local_extensions.containerList + ctry.model.extensions.containerList):
            _info = ctry._countryInfoHandler.GetPieceOfInfo(self.__class__._extensionType,self.ID + el.ID )
            if len(_info) == 0:
                continue
            self.extensions.add(self.ID + el.ID,ExtensionSwitch(_info,el))
    
class ExtensionSwitch(Euromod_Element):
    _objectType = ReadModelOptions.EXTENSIONS
    def _short_repr(self):
        state = "on" if self.baseOff == "false" else "off"
        return f"ExtensionSwitch {self.parent.name}: {state}"
    def __getattr__(self, name):
        if hasattr(self,"parent"):
            return getattr(self.parent,name)
        raise AttributeError(f"Attribute with name {name} not found.")
    def show_attr(self):
        self.parent.show_attr()
        for el in self._info.Keys:
            print(get_cleaned_key(el))
            
class SpineElement(Euromod_Element):
    @property
    def spineOrder(self):
        """:obj:`str`: Order number in the spine."""
        return self._get_spine_order("") #recursively get spineOrder
    def _get_spine_order(self,order_child): #find recursively the spine order
        if not isinstance(self.parent,SpineElement): #Policy Function and Parameter are all SpineElements
            return self.order + order_child
        else:
            return self.parent._get_spine_order( "." + self.order + order_child)
    def __init__(self,*arg):
        super().__init__(*arg)
        self.system_elements = Container()
        """Respective system elements that contain the implementation of the Definition."""
    

class SystemElement(Euromod_Element):
    
    def __init__(self,info,id,parentSystemObject,parentTypeObject,parent):
        self._info = info
        self.ID: str = id
        """System identifier."""
        self.parentSystem = parentSystemObject
        self.parentTypeObject = parentTypeObject
        self.parent = parent
        self._initialised = True
        self.parentTypeObject.system_elements.add(self.ID,self)
    def __getattr__(self,name):
        if hasattr(self,"parentTypeObject"):
            return getattr(self.parentTypeObject,name)
        raise AttributeError(f"Attribute with name {name} not found.")
        
    def show_attr(self):
        self.parentTypeObject.show_attr() #show the available attributes both for parentType
        for el in self._info.Keys:
            print(get_cleaned_key(el))
            
    def __setattr__(self,name,value):
        if "_initialised" in self.__dict__ and (not name in self.__dict__ )and get_csharp_key(name) in self.parentTypeObject._info.Keys:
            self.parentTypeObject.__setattr__( name,value)
        else:
            super().__setattr__(name,value)
    def get_properties(self):
        properties =  list((set(super().get_properties()) | set(self.parentTypeObject.get_properties())) - {"system_elements"})
        properties.sort()
        return properties
        
def get_cleaned_key(key):
    if key != "ID": # Lower first letter if not equal to ID
        key = key[0].lower() + key[1:]
    return key

def get_csharp_key(key):
    if key != "ID": # Lower first letter if not equal to ID
        key = key[0].upper() + key[1:]
    return key


    
    