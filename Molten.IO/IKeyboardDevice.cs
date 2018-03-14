﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    public interface IKeyboardDevice : IInputDevice<Key>
    {
        event KeyPressHandler OnCharacterKey;
    }
}
