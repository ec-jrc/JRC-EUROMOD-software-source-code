API Reference
=============

This reference guide lists the main public objects of the Euromod Connector package. 
The `euromod.core` module contains most of the public classes of the library. 
It provides useful functionalities that allow the user to interact with EUROMOD [#f1]_ and run simulations.
The `euromod.container` module defines a storage class for the model objects accessible by indexing.

Please, refer to the `User Guide <https://euromod-web.jrc.ec.europa.eu/>`_ and `Examples <https://euromod-web.jrc.ec.europa.eu/>`_ for futher readings.

.. toctree::
   :titlesonly:

   {% for page in pages|selectattr("is_top_level_object") %}
   {{ page.include_path }}
   {% endfor %}

.. [#f1] See the documetation for the EUROMOD tax-benefit microsimulation model on the `official webpage <https://euromod-web.jrc.ec.europa.eu/>`_ and in the `resources page <https://euromod-web.jrc.ec.europa.eu/resources>`_.
