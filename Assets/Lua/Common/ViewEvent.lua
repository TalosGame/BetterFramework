--[[
Auth:Chiuan
like Unity Brocast Event System in lua.
]]

local EventLib = require "Common/ooeventlib"
local ViewEvent = Baseclass:New()

function ViewEvent:Init( )
	self.events = {};
end

function ViewEvent:AddListener(event,handler,target)
	--[[if not event or type(event) ~= "string" then
		error("event parameter in addlistener function has to be string, " .. type(event) .. " not right.")
	end--]]
	if not event then return end 
	if not handler or type(handler) ~= "function" then
		-- log("handler parameter in addlistener function has to be function, " .. type(handler) .. " not right")
	end

	if not self.events[event] then
		--create the Event with name
		self.events[event] = EventLib:new(event)
	end

	--conn this handler
	self.events[event]:connect(handler, target)
end

function ViewEvent:Brocast(event,...)
	if self.events[event] then
		print("ViewEvent brocast " .. tostring(event) .. " event.");
		self.events[event]:fire(...)
	else
		print("ViewEvent brocast " .. tostring(event) .. " has no event.")
	end
end

function ViewEvent:RemoveListener(event, handler, target)
	if not self.events[event] then
		print("remove " .. event .. " has no event.")
	else
		self.events[event]:disconnect(handler,target)
		--print("remove success " .. event)
	end
end

function ViewEvent:Log()
	for key, event in pairs( self.events ) do
		print("event key==="..key.." key type=="..type(key).." handle count=="..#event.handlers);
	end
end

function ViewEvent:RemoveAllListeners()
	for k,v in pairs(self.events) do
		v:disconnect()
	end
end

return ViewEvent