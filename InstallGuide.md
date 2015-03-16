# Installation Guide #

## Configure Plugin ##

Download and run the MPE1 file from the [Downloads](http://code.google.com/p/mp-transmission/downloads/list) page.

The following 3 files will be installed in the plugins -> windows directory:

  * mptransmission.dll
  * Jayrock.dll
  * Jayrock.Json.dll

The following files will be installed in your StreamedMP skin directory:

  * mptransmission.xml
  * mptransmission.Details.xml
  * mptransmission.Stats.xml

Open Mediaportal Configuration and configure the mptransmission plugin, you will need to enter the following details as a minimum:

  * Hostname or IP address
  * Port number
  * Authentication details (if used)

Press the test button and ensure that a success message is seen, then press OK to return to the configuration window. If you get an error message, check your configuration details and try again.

## Using Plugin In Mediaportal ##

Once the plugin has been configured as above, open Mediaportal and navigate to the plugin. A list of torrents will be displayed, clicking a torrent brings up its details. Use the context menu to stop/start/remove torrents or to display your server statistics.