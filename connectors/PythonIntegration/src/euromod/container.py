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

import re
import pandas as pd
class Container:
    """
    This class is a container for objects that allow for indexing and representation in multiple ways:
    - via keys that are the name of the objects or,
    - via integer indexing as in a list.
    """
    def __init__(self,idDict=False):
        self.containerDict = {}
        self.containerList = []
        self.idDict = idDict
        if idDict:
            self.dictIds = {}
            
    def add(self,key,value,identifier=None):
        self.containerDict[key] = value
        self.containerList.append(value)
        if self.idDict:
            self.dictIds[identifier] = value
    def _short_repr(self):
        if len(self) > 10:
            return f"{len(self)} elements"
        elif len(self) == 0:
            return "0 elements"
        else:
            rep = ""
            for i,el in enumerate(self):
                if hasattr(el,"_short_repr"):
                    rep += f"{el._short_repr()}"
                elif isinstance(el,pd.DataFrame):
                    rep += f"Pandas DataFrame of {len(el.columns)} variables and {len(el)} observations."
                else:
                    rep += f"{el.__class__.__name__}"
                if i < len(self)-1:
                    rep += ", "
        return rep
    def _get_by_id(self,id):
        if not self.idDict:
            raise Exception("idDict not generated for this type")
        else:
            return self.dictIds[id]
    def __repr__(self):
        s= ""
        maxlen_begin = 0
        maxlen_middle = 0
        end_is_empty = True
        for i,el in enumerate(self.containerList):
            maxlen_begin = maxlen_begin if len(el._container_begin_repr()) + len(str(i)) < maxlen_begin else len(el._container_begin_repr()) + len(str(i))
            repr_middle = el._container_middle_repr()
            maxlen_middle = maxlen_middle if len(repr_middle) + len(str(i)) < maxlen_middle else len(repr_middle) + len(str(i))
            if len(el._container_end_repr()) > 0:
                end_is_empty = False
        for i,el in enumerate(self.containerList):
            name_repr = el._container_begin_repr() + " "*(maxlen_begin - len(el._container_begin_repr()) -len(str(i)))
            repr_middle = el._container_middle_repr() 
            repr_middle_adj = repr_middle + " "*(maxlen_middle - len(repr_middle)) #pretty pritting adjustment middle text
            if maxlen_middle > 0 + len(str(len(self.containerList))):
                s += f"{i}: {name_repr}     | {repr_middle_adj} "
            else:
                s += f"{i}: {name_repr}" 
            if not end_is_empty:
                s += f"    |    {el._container_end_repr()} \n"
            else:
                s += "\n"
        return s
    def __getitem__(self,arg):
        if (type(arg) == int):
            return self.containerList[arg]
        if (type(arg) == slice):
            new_container = Container(self.idDict)
            items_to_select = self.containerList[arg]
            for k,v in self.containerDict.items():
                if v in items_to_select:
                    new_container.containerDict[k] = v
                
            for el in self.containerList:
                new_container.containerList = items_to_select
            if self.idDict:
                for k,v in self.dictIds.items():
                    if v in items_to_select:
                        new_container.dictIds[k] = v
            return new_container
        if type(arg) == str:
            return self.containerDict[arg]
        
    def __setitem__(self,k,v):
        if (type(k) == int) | (type(k) == slice):
            self.containerList[k] = v
            return
        if type(k) == str:
            self.containerDict[k] = v
            return
        
        raise(TypeError("Type of key is not supported"))
    def __iter__(self):
        return iter(self.containerList)
    def __len__(self):
        return len(self.containerList)
    def __add__(self,other):
        new_container = Container(self.idDict)
        for k,v in self.containerDict.items():
            new_container.containerDict[k] = v
            
        for el in self.containerList:
            new_container.containerList.append(el)
        
        for k,v in other.containerDict.items():
            new_container.containerDict[k] = v
        for el in other.containerList:
            new_container.containerList.append(el)
        if self.idDict and other.idDict:
            for k,v in other.dictIds.items():
                new_container.dictIds[k] = v
            for k,v in self.dictIds.items():
                new_container.dictIds[k] = v
            
        return new_container
        
    def keys(self):
        """
        Get keys of the :class:`Container`.
        
        Returns
        -------
        :obj:`Container.keys`
            Names of the attribute or the attribute of a child element.

        """
        return self.containerDict.keys()
    def items(self):
        """
        Get items of the :class:`Container`.
        
        Returns
        -------
        :obj:`Container.items`
            Object items.

        """
        return self.containerDict.items()
    def values(self):
        """
        Get values of the :class:`Container`.
        
        Returns
        -------
        :obj:`Container.values`
            Value of the object attribute.

        """
        return self.containerDict.values()
    def find(self,key,pattern,return_children=False,case_insentive=True):
        """
        Find objects that match pattern.

        Parameters
        ----------
        key : :obj:`str`
            Name of the attribute or the attribute of a child element that you want to look for
            One can search child elements by using the dot-notation. 
            E.g.: mod["BE"]["BE_2023"].policies.find("functions.name","BenCalc")
        pattern : :obj:`str`
            pattern that you want to match.
        return_children : bool, optional
            When True, the return type will be a Container containing elements of the type for which the find method was used
            When False, the return type will be a Container of the elements of the deepest level specified by the pattern key-word.
            E.g.: mod["BE"]["BE_2023"].policies.find("function)
            The default is False.
        case_insentive : :obj:`bool`, optional
            DESCRIPTION. The default is True.

        Returns
        -------
        Container
            An object that matches the pattern.
        """

        return _find(self,key,pattern,return_children,case_insentive=True)

def _find(container,key,pattern,return_children=False,case_insentive=True):
    if len(container) == 0:
        return Container()
    if case_insentive:
        flags = re.I
    else:
        flags = 0
    matches = Container()
    if "." in key:
        idx_dot = key.find(".") #If there is a dot then what is before could be a container
        potential_container = key[:idx_dot]
        if hasattr(container[0],potential_container) and isinstance(getattr(container[0],potential_container), Container):
            for el in container:
                if not hasattr(el,potential_container):
                    continue
                matches_children = _find( getattr(el,potential_container),key[idx_dot+1:],pattern,return_children,case_insentive)
                if return_children:
                    matches += matches_children
                else:
                    if len(matches_children) > 0:
                        matches.add(el.ID,el)
        elif not hasattr(container[0],potential_container):
            raise AttributeError(f"There is no attribute of the type Container with the name {potential_container}")
                    
    else:
         for item in container.containerList:
             if re.search(pattern, getattr(item,key),flags=flags):
                 matches.add(item.name,item)           
    return matches

def filter(container,switches):
    """
    

    Parameters
    ----------
    container : Container
        Elements that need to be filtered.
    switches : Dictionary[str,bool]
        switches that need to be applied to the container.

    Returns
    -------
    ret : Container
        Filtered Container.

    """

    ret = Container()
    for p in container:
        explicit_removal = False
        if len(p.extensions) == 0:
            ret.add(p.ID,p)
            continue
        explicit_removal=False
        explicit_inclusion=False
        for ext in p.extensions:
            if switches[ext.shortName]: 
                if ext.baseOff == "true": #If marked for removal when true
                    explicit_removal = True
                else: #if marked for inclusion when true
                    explicit_inclusion = True
            else:
                if not ext.baseOff == "true": #Mark for exclusion if it is marked for inclusion but not triggered
                    explicit_removal = True


                    
        if explicit_inclusion or not explicit_removal:
            ret.add(p.ID,p)
        
    return ret
        