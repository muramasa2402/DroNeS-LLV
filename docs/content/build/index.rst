Building
=========

This section contains instructions on how to build the project. The following steps assume that you've installed the project prerequisites and have cloned the project repository.

Importing the project
*********************

  1. First open Unity Hub. In the *Projects* pane on the left, click on the top-left *Add* button and add the root folder containing the cloned project repository.

  2. Double-click on the project to open it, selecting **Unity 2018.3.0f2** as the project version.

  3. At this point Unity 3D should take a couple of minutes to import all the project assets and scripts.

Fixing Test Runner errors
*************************

.. note:: At this point there may be one or more errors pertaining to tests written for Mapbox components because Unity does not include test assemblies in compiled builds.

To remove these errors, you need to reference test assemblies from all the assemblies:

  1. Go to **Window** -> **General** -> **Test Runner**
  2. Click the small drop-down menu in the top-right of the window.
  3. Click **Enable playmode tests for all assemblies**
  4. In the dialog box appears, click **OK** to manually restart the Editor.

.. image:: ../../_static/img/UnityTestRunner.png

After relaunching the project there should be no more errors.

Building the project
********************

  1. Go to **File** -> **Build Settings...** (alternative press ``CTRL + SHIFT + B``)
  2. Configure the build settings choosing the appropriate target platform and architecture of choice. The following platforms have been tested:

        * Windows x86_64
        * Windows x86

  3. Click **Build and Run** and specify the build output folder
  4. Wait for the build to complete

Once complete, the output folder should contain a `city_v1.exe` executable where the simulation program can be launched.
