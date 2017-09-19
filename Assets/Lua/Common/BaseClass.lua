local Baseclass = {};

function Baseclass:New(o)    
    o = o or {};
    setmetatable(o, { __index = self });     
    return o;  
end

return Baseclass;