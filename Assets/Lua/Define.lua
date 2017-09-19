ResDefine = require "ResDefine"
EventConst = require "EventConst"

Baseclass = require 'Common/Baseclass'
Event = require "Common/ViewEvent"

-- global class
ViewEvent = Event:New();
ViewEvent:Init();

-- Bean lua define
UserBean = require "UI/Bean/UserBean"

-- Control lua define
LUserManager = require "UI/Control/LUserManager"