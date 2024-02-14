-- Delete pixels in a range from a selected layer or all layers

local spr = app.activeSprite
if not spr then return print('No active sprite') end

local dlg = Dialog("Rename Layers")
dlg:entry{ id="new_name", label="New Name:"}
dlg:combobox{ id="type", label="Rename Type:", options={"Full Rename", "Prepend", "Append"}}
dlg:separator{}
dlg:combobox{ id="layer_select", label="Rename Layers:", options={"All Layers", "Selected Layers", "Create New Layer"}}
dlg:button{ id="rename", text="Rename" }
dlg:button{ id="cancel", text="Cancel" }
dlg:show()

local data = dlg.data

if data.rename then
  local confirmationMsg = {}
  if data.type == "Full Rename" then
    table.insert(confirmationMsg, "Rename following layers to \"" .. data.new_name .. "\"?")
  elseif data.type == "Prepend" then
    table.insert(confirmationMsg, "Prepend \"" .. data.new_name .. "\" to the following layer names?")
  else
    table.insert(confirmationMsg, "Append \"" .. data.new_name .. "\" to the following layer names?")
  end
  
  local layers = {}
  if data.layer_select == "All Layers" then
    for i,layer in ipairs(spr.layers) do
      table.insert(layers, layer)
      table.insert(confirmationMsg, "- " .. layer.name)
    end
  elseif data.layer_select == "Selected Layers" then
    for i, layer in ipairs(app.range.layers) do
      table.insert(layers, layer)
      table.insert(confirmationMsg, "- " .. layer.name)
    end
  else
    confirmationMsg = "Create new layer with name \"" .. data.new_name .. "\"?"
  end

  if app.alert{ title="Confirm...", text=confirmationMsg,
              buttons={ "&Yes", "&No" } } ~= 1 then
  return
  end

  if data.layer_select == "Create New Layer" then
    local newLayer = spr:newLayer()
    newLayer.name = data.new_name
  else
    for i, layer in ipairs(layers) do
      if data.type == "Full Rename" then
        layer.name = data.new_name
      elseif data.type == "Prepend" then
        layer.name = data.new_name .. layer.name
      else
        layer.name = layer.name .. data.new_name
      end
    end
  end
  
  if app.alert{ title="Finished...", text="Rename Complete.",
                buttons={ "&OK" } } ~= 1 then
    return
    end
end